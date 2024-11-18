
use core::hash;
use std::{array, convert, num, path, path::Path};
use std::cmp::min_by;
use std::env::consts::EXE_SUFFIX;
use std::fs::File;
use std::io::{BufReader, BufWriter, Read, Write};

use pruefung::crc::Crc32;
use serde::{Deserialize, Serialize};
use serde_json::map::Values;
use serde_json::{json, to_string, Map, Value};

use ndarray::{arr1, Array1, Array2, ArrayBase, Axis, Dim, Dimension, Shape, ShapeArg, ShapeBuilder};

use rand::Rng;

use pruefung::{crc, Digest, Hasher};

#[derive(Deserialize)]
#[derive(Debug)]
struct Story{
    filename: String,
    words: Vec<String>,
    text: String
}

#[derive(Debug)]
struct SVD_Entry
{
    singular_value: f32,
    u: Array2<f32>,
    v: Array2<f32>
}

#[derive(Serialize, Deserialize)]
struct Word_Count_Metadata
{
    hash:u64,
    rows:usize,
    cols:usize
}

impl Word_Count_Metadata
{
    fn compute_hash(filename:&str) -> u64
    {
        let mut hasher = Crc32::new();
        let file = File::open(filename).unwrap();
        let mut reader: BufReader<File> = std::io::BufReader::new(file);
        let mut bytes = vec![];

        let file_read = reader.read_to_end(&mut bytes);

        hasher.write(&bytes);

        return hasher.finish();
    }

    fn from_file(filename:&str) -> Word_Count_Metadata
    {
        let file= File::open(filename);
        let hash = Word_Count_Metadata::compute_hash(filename);
        let metadata = Word_Count_Metadata{hash: hash, rows: 0, cols: 0};

        return metadata;
    }

    fn matches_file(&self, filename:&str) -> Result<(), u64>
    {
        let hash = Word_Count_Metadata::compute_hash(filename);

        if self.hash == hash
        {
            return Result::Ok(());
        }
        else
        {
            return Result::Err(hash);
        }
    }

    fn read_from_file(filename:&str) -> Word_Count_Metadata
    {
        let file = File::open(filename).unwrap();
        let reader:BufReader<File> = std::io::BufReader::new(file);
        let deserialized_data:Word_Count_Metadata = serde_json::from_reader(reader).unwrap();
        return deserialized_data;
    }

    fn write_to_file(&self, filename:&str)
    {
        let file = File::create(filename).unwrap();
        let writer = std::io::BufWriter::new(file);
        serde_json::to_writer(writer, self).unwrap();
    }
}

#[derive(Serialize, Deserialize)]
struct NDArray_Serializable
{
    data:Vec<f32>,
    num_rows: i32,
    num_cols: i32
}

impl NDArray_Serializable
{
    fn from_array2(A:&Array2<f32>) -> NDArray_Serializable
    {
        let mut data: Vec<f32> = vec![];
        let mut num_entries = 0;
        let mut total_word_count = 0.0;
        for row in A.rows().into_iter()
        {
            for entry in row.iter()
            {
                total_word_count += entry;
                num_entries += 1;
                data.push(*entry);
            }
        }
        println!("{0} entries, {1} words total", num_entries, total_word_count);
        let num_rows = A.len_of(Axis(0)) as i32;
        let num_cols = A.len_of(Axis(1)) as i32;

        return NDArray_Serializable{data, num_rows, num_cols};
    }

    fn to_array2(&self) -> Array2<f32>
    {
        println!(
            "creating Array2<f32> with {0} rows and {1} columns",
            self.num_rows,
            self.num_cols
        );

        let shape = (self.num_rows as usize, self.num_cols as usize);
        
        let A = Array2::from_shape_vec(shape, self.data.clone()).unwrap();

        return A;
    }

    fn write_to_file(&self, filename: &str)
    {
        let file = File::create(filename).unwrap();
        let writer: BufWriter<File> = std::io::BufWriter::new(file);
        serde_json::to_writer(writer, self).unwrap();
    }

    fn read_from_file(filename:&str) -> Array2<f32>
    {
        let file = File::open(filename).unwrap();
        let reader:BufReader<File> = std::io::BufReader::new(file);
        let deserialized_data:NDArray_Serializable = serde_json::from_reader(reader).unwrap();
        return deserialized_data.to_array2();
    }

}



fn normalize(vector:&Array2<f32>) -> Array2<f32>
{
    let length = norm(vector);
    let normal_vector = vector / length;

    return normal_vector;
}

fn norm(vector:&Array2<f32>) -> f32
{
    return vector.mapv(|n| n.powi(2)).sum().sqrt();
}

fn multiply_vector_until_convergence(vector: &mut Array2<f32>, array:&Array2<f32>, epsilon:f32) -> bool
{
    let old_vector = vector.clone();
    let mut new_vector = array.dot(vector);
    new_vector = normalize(&new_vector);
    
    let proj = norm(&new_vector.clone().reversed_axes().dot(&old_vector));
    println!("{0}", proj);
    let has_not_converged:bool = proj < (1.0 - epsilon);
    *vector = new_vector;

    return has_not_converged;
}

fn random_unit_vector(size:usize) -> Array2<f32>
{
    let mut vector: Array2<f32> = ndarray::Array2::zeros((size, 1));
    let mut rng = rand::thread_rng();

    vector = vector.map(|_| rng.gen());

    let norm = norm(&vector);
    vector = vector / norm;

    return vector;
}

fn oneD_SVD(A: &Array2<f32>, epsilon:f32) -> Array2<f32>
{  
    //matrix A has m rows and n columns
    let m = A.len_of(Axis(0));
    let n = A.len_of(Axis(1));
    //we're going to do a dot product of A and A transpose
    //in whichever order gives us a smaller resulting matrix
    //which will be a square with both min_size rows and columns
    let min_size = core::cmp::min(m, n);

    let A_T = A.clone().reversed_axes();
    let B:Array2<f32>;

    if m > n
    {
        B = A_T.dot(A);
    }
    else
    {
        B = A.dot(&A_T);
    }

    let mut v = random_unit_vector(min_size);
    while multiply_vector_until_convergence(&mut v, &B, epsilon)
    {
        println!("{:?}", v.to_string());
    }

    return v;
}

fn exclude_span_of_vector(A:&mut Array2<f32>, svd_entry:&SVD_Entry)
{
    let sigma = &svd_entry.singular_value;
    let u = &svd_entry.u;
    let v = &svd_entry.v;

    let size = A.len_of(Axis(1));
    for i in 0..size
    {
        for j in 0..size
        {
            A[(i, j)] -= sigma * u[(i, 0)] * v[(j, 0)];
        }
    }
}

fn generate_word_counts(A: &mut Array2<f32>, words:&Vec<String>, stories:&Vec<Story>)
{
    for (current_col, story) in stories.iter().enumerate(){
        for story_word in &story.words{
            for (current_row, word) in words.iter().enumerate(){
                if story_word == word {
                    A[(current_row, current_col)] += 1.0;
                    break;
                }
            }
        }
    }
}

fn main() {
    let STORY_FILENAME = "all_stories.json";
    let WORDS_FILENAME= "one-grams.txt";
    let MATRIX_DATA_FILENAME = "matrix_data.json";
    let MATRIX_INFO_FILENAME = "matrix_info.json";

    //read stories from json
    let mut story_file = File::open(STORY_FILENAME).unwrap();
    let mut story_file_contents = String::new();
    story_file.read_to_string(&mut story_file_contents).unwrap();
    let stories: Vec<Story> = serde_json::from_str(&story_file_contents).unwrap();
    
    //read words from txt
    let mut words_file: File = File::open(WORDS_FILENAME).unwrap();
    let mut words_file_contents: String = String::new();
    words_file.read_to_string(&mut words_file_contents).unwrap();
    let words: Vec<String> = words_file_contents.lines()
        .map(|word| word.to_string()).collect();

    //word_counts has a row for each word, and a column for each story
    //each entry represents the number of times that column's word appears in that row's story
    //doing the counting every time takes a while and makes debugging boring
    //so as long as all_stories.json hasn't been changed, we can just read in the data
    let m = words.len();
    let n = stories.len();
    let mut word_counts = Array2::<f32>::zeros((m, n));

    let matrix_info_exists = Path::new(MATRIX_INFO_FILENAME).exists();
    let matrix_data_exists = Path::new(MATRIX_DATA_FILENAME).exists();
    let mut matrix_info = Word_Count_Metadata{hash: 0, rows: 0, cols: 0};
    
    //stays true if files didn't exist previously OR hash didn't match
    let mut calculate_word_counts = true;
    //stays true if files didn't exist previously
    //checking the hash calculates it
    let mut calculate_hash = true;

    println!("checking on files...");
    //both files exist, but we don't know yet if the hash matches
    if matrix_info_exists && matrix_data_exists
    {
        println!("matrix files present, checking hash...");
        matrix_info = Word_Count_Metadata::read_from_file(MATRIX_INFO_FILENAME);

        let hash_check = matrix_info.matches_file(STORY_FILENAME);

        if hash_check.is_ok()
        {
            println!("hash matches! using {0} unchanged", MATRIX_DATA_FILENAME);
            word_counts = NDArray_Serializable::read_from_file(MATRIX_DATA_FILENAME);

            calculate_hash = false;
            calculate_word_counts = false;
        }
        else 
        {
            println!("hash doesn't match :(");
            //need to recalculate matrix_data.json, but hang onto this hash
            //instead of figuring it out twice
            let hash_value = hash_check.unwrap_err();
            matrix_info.hash = hash_value;

            calculate_hash = false;
            calculate_word_counts = true;
        }
    }

    if calculate_hash
    {
        matrix_info = Word_Count_Metadata::from_file(STORY_FILENAME);
    }

    if calculate_word_counts
    {
        generate_word_counts(&mut word_counts, &words, &stories);

        matrix_info.rows = words.len();
        matrix_info.cols = stories.len();
    
        matrix_info.write_to_file(MATRIX_INFO_FILENAME);
    
        let matrix_data = NDArray_Serializable::from_array2(&word_counts);
        matrix_data.write_to_file(MATRIX_DATA_FILENAME);  
    }     


    //using power method for finding largest eigenvector / eigenvalue
    
    let mut svd_so_far:Vec<SVD_Entry> = vec![];
    let epsilon:f32 = (10.0_f32).powi(-3);


    for iteration in 0..10
    {
        println!("ITERATION {0}", iteration);

        let mut A = word_counts.clone();
        
        //exclude span of already found vectors
        for svd_entry in &svd_so_far{
            exclude_span_of_vector(&mut A, &svd_entry);
        }

        //find next singular vector
        let v = oneD_SVD(&A, epsilon);
        let mut u:Array2<f32> = word_counts.dot(&v);
        let sigma = norm(&u);
        u = u / sigma;
    
        let new_svd_entry = SVD_Entry{singular_value: sigma, u: u, v: v};

        println!("{0}", new_svd_entry.singular_value);
        svd_so_far.push(new_svd_entry);
    }

}