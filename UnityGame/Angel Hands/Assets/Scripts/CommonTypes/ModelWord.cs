using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Assets.Scripts.CommonTypes
{
    public class ModelWord
    {
        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("urls")]
        public List<string> Urls { get; set; }

        [JsonProperty("recordings_path")]
        public string RecordingsPath { get; set; }

        // Constructor for initialization (optional)
        public ModelWord()
        {
            Urls = new List<string>();
        }

        // Method to load JSON from a file and deserialize into a list of ModelWord objects
        public static List<ModelWord> LoadFromFile(string filePath)
        {
            // Read the entire JSON file
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON content into a list of ModelWord objects using Newtonsoft.Json
            return JsonConvert.DeserializeObject<List<ModelWord>>(json);
        }
    }
}
