using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json; // Include this for JSON serialization (Newtonsoft.Json)
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.CommonTypes;
using Unity.VisualScripting;
using System;

namespace Assets.Scripts.User.playfab
{
	public class PlayFabWordManager 
	{
        public static List<Word> wordsLearned = null;
        public static List<GameSession> history = new List<GameSession>();

        #region saving words learned
        // Call this to save the list of learned words
        public static void SaveWordsLearned()
		{
            // Serialize the list to a JSON string
            string json = JsonConvert.SerializeObject(wordsLearned);

			// Create request to save player data
			var request = new UpdateUserDataRequest
			{
				Data = new Dictionary<string, string> {
				{ "WordsLearned", json } // Save the JSON string under the key "WordsLearned"
            }
			};

			PlayFabClientAPI.UpdateUserData(request, OnDataSendSuccess, OnDataSendFailure);
		}

        static void OnDataSendSuccess(UpdateUserDataResult result)
		{
			Debug.Log("Data saved successfully!");
		}

        static void OnDataSendFailure(PlayFabError error)
		{
			Debug.LogError("Failed to save data: " + error.GenerateErrorReport());
		}
        #endregion
        #region save history
        public static void SaveHistory()
        {
            Debug.Log("Saving history");
            // Serialize the list to a JSON string
            string json = JsonConvert.SerializeObject(history);

            Debug.Log("History: " + json);
            // Create request to save player data
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> {
                { "GameSessionsHistory", json } // Save the JSON string under the key "WordsLearned"
            }
            };

            PlayFabClientAPI.UpdateUserData(request, OnHistoryDataSendSuccess, OnHistoryDataSendFailure);
            
        }

        static void OnHistoryDataSendSuccess(UpdateUserDataResult result)
        {
            Debug.Log("History Data saved successfully!");
        }

        static void OnHistoryDataSendFailure(PlayFabError error)
        {
            Debug.LogError("Failed to save History data: " + error.GenerateErrorReport());
        }
        #endregion

        #region loading learned words
        static public void LoadWordsLearned()
        {
             PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnDataReceiveFailed);
        }

        static void OnDataReceived(GetUserDataResult result)
        {
            if (result.Data != null && result.Data.ContainsKey("WordsLearned"))
            {
                // Deserialize the JSON string back into a List<Word>
                string json = result.Data["WordsLearned"].Value;

                wordsLearned = JsonConvert.DeserializeObject<List<Word>>(json);

                Debug.Log("Words learned loaded successfully!");
            }
            else
            {
                wordsLearned= new List<Word>();

                Debug.LogWarning("No data found for WordsLearned.");
            }
        }

        static void OnDataReceiveFailed(PlayFabError error)
        {
            Debug.LogError("Failed to load data: " + error.GenerateErrorReport());
        }
        #region loading history
        static public void loadUserHistory()
        {

           PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnHistoryDataReceived, OnHistoryDataReceiveFailed);

        }
        #endregion

        private static void OnHistoryDataReceived(GetUserDataResult result)
        {
            if (result.Data != null && result.Data.ContainsKey("GameSessionsHistory"))
            {
                // Deserialize the JSON string back into a List<Word>
                string json = result.Data["GameSessionsHistory"].Value;

                history = JsonConvert.DeserializeObject<List<GameSession>>(json);

                Debug.Log("History loaded successfully!");
                Debug.Log("History Loaded: " + json);
            }
            else
            {
                history = new List<GameSession>();//tmp
                Debug.LogWarning("No data found for History.");
            }
        }

        private static void OnHistoryDataReceiveFailed(PlayFabError error)
        {
            Debug.LogError("Failed to load data: " + error.GenerateErrorReport());
        }

        #endregion

    }
}