
use std::{array, convert, num};
use std::cmp::min_by;
use std::env::consts::EXE_SUFFIX;
use std::fs::File;
use std::io::Read;

use serde::{Deserialize, Serialize};
use serde_json::map::Values;
use serde_json::{json, to_string, Map, Value};

use ndarray::{arr1, Array1, Array2, ArrayBase, Axis, Dim, Dimension, Shape, ShapeArg, ShapeBuilder};

use rand::Rng;

#[derive(Deserialize)]
#[derive(Debug)]
struct Story{
    filename: String,
    words: Vec<String>,
    text: String
}

#[derive(Debug)]
struct SVD_Entry{
    singular_value: f32,
    u: Array2<f32>,
    v: Array2<f32>
}

#[derive(Serialize, Deserialize)]
struct NDArray_Serializable
{
    data:Vec<f32>,
    num_rows: i32,
    num_cols: i32,
}

impl NDArray_Serializable
{
    fn from_array2(A:&Array2<f32>) -> NDArray_Serializable
    {
        let mut data: Vec<f32> = vec![];

        for row in A.rows().into_iter()
        {
            for entry in row.iter()
            {
                data.push(entry.clone());
            }
        }
    
        let num_rows = A.len_of(Axis(0)) as i32;
        let num_cols = A.len_of(Axis(1)) as i32;

        return NDArray_Serializable{data, num_rows, num_cols};
    }

    fn to_array2(&self) -> Array2<f32>
    {
        let shape = (self.num_rows as usize, self.num_cols as usize);
        
        let A = Array2::from_shape_vec(shape, self.data.clone()).unwrap();

        return A;
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

fn exclude_span_of_vector(A:&mut Array2<f32>, u:&Array2<f32>, v:&Array2<f32>)
{
    let size = A.dim().0;
    for i in 0..size
    {
        for j in 0..size
        {
            A[(i, j)] -= u[(0, i)] * v[(0, j)];
        }
    }
}

fn write_word_counts_file(A: &Array2<f32>)
{

}

fn main() {
    //read stories from json
    let mut story_file = File::open("all_stories.json").unwrap();
    let mut story_file_contents = String::new();
    story_file.read_to_string(&mut story_file_contents).unwrap();
    let stories: Vec<Story> = serde_json::from_str(&story_file_contents).unwrap();
    
    //read words from txt
    let mut words_file: File = File::open("one-grams.txt").unwrap();
    let mut words_file_contents: String = String::new();
    words_file.read_to_string(&mut words_file_contents).unwrap();
    let most_common_words: Vec<String> = words_file_contents.lines()
        .map(|word| word.to_string()).collect();

    //word_counts has a row for each story, and a column for each word
    //each entry represents the number of times that column's word appears in that row's story
    let mut word_counts = Array2::<f32>::zeros((most_common_words.len(), stories.len()));
    for (current_col, story) in stories.iter().enumerate(){
        for story_word in &story.words{
            for (current_row, word) in most_common_words.iter().enumerate(){
                if story_word == word {
                    word_counts[(current_row, current_col)] += 1.0;
                    break;
                }
            }
        }
    }

    let test = NDArray_Serializable::from_array2(&word_counts);
    println!("{0}x{1} array", test.num_rows, test.num_cols);

    //using power method for finding largest eigenvector / eigenvalue
    
    let mut svd_so_far:Vec<SVD_Entry> = vec![];
    let epsilon:f32 = (10.0_f32).powi(-3);


    for iteration in 0..10
    {
        println!("ITERATION {0}", iteration);

        let mut A = word_counts.clone();
        
        //exclude span of already found vectors
        for svd_entry in &svd_so_far{
            exclude_span_of_vector(&mut A, &svd_entry.u, &svd_entry.v);
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