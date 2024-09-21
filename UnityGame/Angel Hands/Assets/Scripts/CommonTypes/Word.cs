using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Assets.Logger;
using System.Linq;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.CommonTypes
{
    [System.Serializable]
    public class Word
    {
        public string word; // the word itself
        public int times_learned; // how many times the instruction video was watched
        public Dictionary<long, WordAttempt> history; // <sessionID, WordAttempt>
        public bool is_learned; // notify if the sign was already learned (times learned > 0 and succeeded > 0)
        public string videoDirectoryPath; // link to video
        public List<string> youtubeLinks;

        // Default constructor (required for JSON deserialization)
        public Word()
        {
        }

        // Constructor for adding a new word to the JSON file
        public Word(string word, string url)
        {
            this.word = word;
            this.videoDirectoryPath = url;
            this.history = new Dictionary<long, WordAttempt>();
            this.is_learned = false;
            this.times_learned = 0;
            this.youtubeLinks = new List<string>();
            //SaveWordToAvailableWordsJson();
        }

        // Constructor for adding a new word to the JSON file
        public Word(ModelWord modelWord)
        {
            this.word = modelWord.Word;
            this.videoDirectoryPath = modelWord.RecordingsPath;
            this.history = new Dictionary<long, WordAttempt>();
            this.is_learned = false;
            this.times_learned = 0;
            this.youtubeLinks = modelWord.Urls ?? new List<string>();
            //SaveWordToAvailableWordsJson();
        }

        // Constructor for creating a word with all properties
        public Word(string word, int times_learned, Dictionary<long, WordAttempt> history, bool is_learned, string url)
        {
            this.word = word;
            this.times_learned = times_learned;
            this.history = history;
            this.is_learned = is_learned;
            this.videoDirectoryPath = url;
        }

        // Method to save the word to the JSON file
        private void SaveWordToAvailableWordsJson()
        {
            // Load existing words
            List<Word> words = LoadWordsFromJson("words.json");

            // Add new word
            words.Add(this);

            // Serialize back to JSON
            string jsonString = JsonConvert.SerializeObject(words, Formatting.Indented);
            File.WriteAllText("words.json", jsonString);
        }



        // Static method to load words from a JSON file
        public static List<Word> LoadWordsFromJson(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    List<Word> words = JsonConvert.DeserializeObject<List<Word>>(jsonString);
                    return words;
                }
                else
                {
                    FileLogger.LogError("File not found.");
                    return new List<Word>();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"An error occurred while loading the file: {ex.Message}");
                return new List<Word>();
            }
        }

        //// Method to save the entire list of words to a JSON file
        //public static void SaveWordsToJson(string filePath, List<Word> words)
        //{
        //    try
        //    {
        //        string jsonString = JsonConvert.SerializeObject(words, Formatting.Indented);
        //        File.WriteAllText(filePath, jsonString);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while saving to the file: {ex.Message}");
        //    }
        //}

        public static void SaveWordsToJson(string filePath, List<Word> words)
        {
            try
            {
                // Backup the original file if it exists
                if (File.Exists(filePath))
                {
                    string backupFilePath = Path.Combine(
                        Path.GetDirectoryName(filePath),
                        Path.GetFileNameWithoutExtension(filePath) + "_bak" + Path.GetExtension(filePath)
                    );

                    // If a backup file already exists, overwrite it
                    if (File.Exists(backupFilePath))
                    {
                        File.Delete(backupFilePath);
                    }

                    File.Move(filePath, backupFilePath);
                }

                // Serialize the list of words to a JSON string
                string jsonString = JsonConvert.SerializeObject(words, Formatting.Indented);

                // Write the JSON string to the original file (overwrite or create new)
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving to the file: {ex.Message}");
            }
        }

        // Method to update an existing word in the JSON file
        public static bool UpdateWordInJson(string filePath, Word updatedWord)
        {
            try
            {
                List<Word> words = LoadWordsFromJson(filePath);

                // Find the word to update
                int index = words.FindIndex(w => w.word.Equals(updatedWord.word, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    // Update the word properties
                    words[index] = updatedWord;

                    // Serialize back to JSON
                    string jsonString = JsonConvert.SerializeObject(words, Formatting.Indented);
                    File.WriteAllText(filePath, jsonString);

                    return true; // Update successful
                }
                else
                {
                    Console.WriteLine("Word not found.");
                    return false; // Word not found
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while updating the file: {ex.Message}");
                return false;
            }
        }

        public static List<Word> LoadWordsFromModelWords(List<ModelWord> modelWords)
        {
            List<Word> words = new List<Word>();
            foreach (ModelWord modelWord in modelWords)
            {
                Word word = new Word(modelWord);
                words.Add(word);
            }
            return words;
        }

        public static List<Word> UpdateWordsFromModelWords(List<ModelWord> modelWords, List<Word> existingWords = null)
        {
            existingWords ??= new List<Word>();

            foreach (var modelWord in modelWords)
            {
                var existingWord = existingWords.FirstOrDefault(w => w.word.Equals(modelWord.Word, StringComparison.OrdinalIgnoreCase));

                if (existingWord != null)
                {
                    existingWord.videoDirectoryPath = modelWord.RecordingsPath;
                    existingWord.youtubeLinks = modelWord.Urls ?? new List<string>();
                }
                else
                {
                    var newWord = new Word(modelWord);
                    existingWords.Add(newWord);
                }
            }

            return existingWords;
        }
        public override bool Equals(object obj)
        {
        
            if (obj == null || !(obj is Word)) return false;
        
            Word other = (Word)obj;
        
            // Define the equality condition (e.g., words with the same word string are considered equal)
            return this.word.Equals(other.word, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
