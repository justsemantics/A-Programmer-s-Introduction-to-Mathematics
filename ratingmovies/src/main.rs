
use std::fs::File;
use std::io::Read;

use serde::Deserialize;
use serde_json::map::Values;
use serde_json::{json, to_string, Map, Value};

use ndarray::{arr1, Array1, Array2, ArrayBase};

use rand::Rng;

#[derive(Deserialize)]
#[derive(Debug)]
struct Story{
    filename: String,
    words: Vec<String>,
    text: String
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
    let mut new_vector = array.dot(vector);
    new_vector = normalize(&new_vector);
    

    *vector = new_vector;

    return false;
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

    let word_copy = word_counts.clone();
    word_counts = word_counts.reversed_axes().dot(&word_copy);

    //using power method for finding largest eigenvector / eigenvalue
    let mut first_vector = random_unit_vector(stories.len());

    let EPS:f32 = (10.0_f32).powi(-6);
    while multiply_vector_until_convergence(&mut first_vector, &word_counts, EPS)
    {
        println!("{:?}", first_vector.to_string());
    }
}