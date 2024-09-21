namespace Assets.Scripts.CommonTypes
{
    public class WordResponse
    {
        public string Word { get; private set; } // The word presented to the player
        public float ResponseTime { get; private set; } // The time it took to respond
        public bool Skipped { get; private set; } // Whether the word was skipped

        // Constructor for a word response
        public WordResponse(string word, float responseTime, bool skipped)
        {
            Word = word;
            ResponseTime = responseTime;
            Skipped = skipped;
        }
    }
}