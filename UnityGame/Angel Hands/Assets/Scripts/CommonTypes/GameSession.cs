using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using Assets.Scripts.User.playfab;

namespace Assets.Scripts.CommonTypes
{
    public class GameSession
    {
        private static readonly string GameSessionsFilePath = Path.Combine(Application.streamingAssetsPath,"UserStatistics", "GameSessions.json");
        public string SessionId { get; set; }
        public string GameMode { get; set; }
        public DateTime SessionDate { get; set; }

        public List<WordResponse> WordResponses = new List<WordResponse>();

        public int maxScore { get; set; } = 0;
        public int score { get; set; } = 0;

        public int NumberOfWordsInGame { get; set; } = 10;

        public GameSession(int numberOfWordsInGame,string gameMode, int CorrectAnswerScore = 100)
        {
            SessionId = Guid.NewGuid().ToString();
            SessionDate = DateTime.Now;
            WordResponses = new List<WordResponse>();
            NumberOfWordsInGame = numberOfWordsInGame;
            maxScore = CorrectAnswerScore * numberOfWordsInGame;
            GameMode = gameMode;
        }

        public void AddWordResponse(string word, float responseTime, bool skipped)
        {
            WordResponses.Add(new WordResponse(word, responseTime, skipped));
        }

        public void SaveGameSessionToDB()
        {
            
            List<GameSession> gameSessions = LoadAllGameSessions();
            
            if (User.User.Instance.isUserLoggedIn)
            {
                PlayFabWordManager.history.Add(this);
                return;
            }
            gameSessions.Add(this);

            string json = JsonConvert.SerializeObject(gameSessions, Formatting.Indented);
            File.WriteAllText(GameSessionsFilePath, json);
        }

        public static void SaveGameSessionAsync(GameSession gameSession)
        {
            // Fire-and-forget task to save the game session asynchronously
            Task.Run(() => gameSession.SaveGameSessionToDB());
        }

        public static List<GameSession> LoadAllGameSessions()
        {
            if (User.User.Instance.isUserLoggedIn)
            {
                return PlayFabWordManager.history;
            }
            if (!File.Exists(GameSessionsFilePath))
            {
                File.WriteAllText(GameSessionsFilePath, "[]");  
            }
            string json = File.ReadAllText(GameSessionsFilePath);
            return JsonConvert.DeserializeObject<List<GameSession>>(json) ?? new List<GameSession>();
        }

        public static GameSession GetSessionById(string sessionId)
        {
            List<GameSession> gameSessions = LoadAllGameSessions();
            return gameSessions.FirstOrDefault(session => session.SessionId == sessionId);
        }

        public static List<string> GetSessionIdListForMode(string gameMode)
        {
            List<GameSession> gameSessions = LoadAllGameSessions();
            return gameSessions
                .Where(session => session.GameMode == gameMode)
                .Select(session => session.SessionId)
                .ToList();
        }

        public static List<GameSession> GetSessionsForMode(string gameMode)
        {
            List<GameSession> gameSessions = LoadAllGameSessions();
            return gameSessions.Where(session => session.GameMode == gameMode).ToList();
        }

        public static List<GameSession> GetAllGameSessions()
        {
            return LoadAllGameSessions();
        }
    }
}
