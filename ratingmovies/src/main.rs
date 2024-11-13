use std::{array, vec};
use std::fs::File;
use std::io::Read;
use serde::Deserialize;
use serde_json::map::Values;
use serde_json::{json, to_string, Map, Value};

#[derive(Deserialize)]
#[derive(Debug)]
struct Story{
    filename: String,
    words: Vec<String>,
    text: String
}

fn main() {
    let mut story_file = File::open("all_stories.json").unwrap();
    let mut story_file_contents = String::new();
    story_file.read_to_string(&mut story_file_contents).unwrap();
    let stories: Vec<Story> = serde_json::from_str(&story_file_contents).unwrap();
    
    let mut words_file: File = File::open("one-grams.txt").unwrap();
    let mut words_file_contents: String = String::new();
    words_file.read_to_string(&mut words_file_contents).unwrap();
    let most_common_words: Vec<String> = words_file_contents.lines()
        .map(|word| word.to_string()).collect();
    let mut word_counts = ndarray::Array2::<i32>::zeros((most_common_words.len(), stories.len()));
    for (current_col, story) in stories.iter().enumerate(){
        for story_word in &story.words{
            for (current_row, word) in most_common_words.iter().enumerate(){
                if story_word == word {
                    word_counts[(current_row, current_col)] += 1;
                    println!("[{0}, {1}]: \"{2}\" occurs {3} times in {4}",
                        current_row,
                        current_col,
                        word,
                        word_counts[(current_row, current_col)],
                        story.filename);
                    break;
                }
            }
        }
    }

    let sub_array:ndarray::ArrayBase<ndarray::OwnedRepr<i32>, ndarray::Dim<[usize; 2]>> = word_counts.select(ndarray::Axis(0), &[0]);
    println!("{:?}", sub_array.to_string())
}