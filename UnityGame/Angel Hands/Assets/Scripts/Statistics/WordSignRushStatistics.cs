
using Assets.Scripts.CommonTypes;
using ReadyPlayerMe.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Assets.Scripts.Statistics {
    class WordSignRushStatistics
    {
        public static string currentWord;

        public static List<GameSession> gameSessions = new List<GameSession>();


        public static void getGameSessions()
        {
            gameSessions = GameSession.LoadAllGameSessions();
        }

        public static int getSkippedStatus()
        {
            int skipCount = 0;
            foreach (GameSession gameSession in gameSessions)
            {
                //count skip
                foreach (WordResponse wordResponse in gameSession.WordResponses)
                {
                    if (currentWord == wordResponse.Word) {
                        if (wordResponse.Skipped)
                        {
                            skipCount++;
                        }
                    }
                }

            }
            return skipCount;
        }
        public static int getNotSkippedStatus()
        {
            int notskipCount = 0;
            foreach (GameSession gameSession in gameSessions)
            {
                //count skip
                foreach (WordResponse wordResponse in gameSession.WordResponses)
                {
                    if (currentWord == wordResponse.Word && !wordResponse.Skipped)
                    {
                        notskipCount++;
                    }
                }

            }
            return notskipCount;
        }


        public static Dictionary<DateTime, float> getResponseTimes()
        {
            Dictionary<DateTime, float> responses = new Dictionary<DateTime, float>();
            foreach (GameSession gameSession in gameSessions)
            {
                foreach (WordResponse wordResponse in gameSession.WordResponses)
                {
                    if (currentWord == wordResponse.Word)
                    {
                        if (!responses.ContainsKey(gameSession.SessionDate))
                        {
                            UnityEngine.Debug.Log("Time to recognize: " + gameSession.SessionDate);
                            responses.Add(gameSession.SessionDate, wordResponse.ResponseTime);
                            break;
                        }
                        
                    }
                }
            }
            return responses;
        } 

        public static Dictionary<DateTime, float> getScores()
        {
            Dictionary<DateTime, float> scores = new Dictionary<DateTime, float>();
            foreach (GameSession gameSession in gameSessions)
            {
                scores.Add(gameSession.SessionDate, gameSession.score);
            }
            return scores;
        }

        public static Dictionary<string, int> wordsFrequency()
        {
            Dictionary<string, int> wordsFrequency = new Dictionary<string, int>();

            foreach (GameSession gameSession in gameSessions)
            {
                foreach (WordResponse wordResponse in gameSession.WordResponses)
                {
                    if (wordResponse.Skipped == true)
                    {
                        continue;
                    }
                    // If the word exists, increment its count; otherwise, add it with a count of 1
                    if (wordsFrequency.ContainsKey(wordResponse.Word))
                    {
                        wordsFrequency[wordResponse.Word]++;
                    }
                    else
                    {
                        wordsFrequency[wordResponse.Word] = 1;
                    }
                }
            }

            return wordsFrequency;
        }

        public static Dictionary<string, int> GetTop5MostCommonWords()
        {
            // Get the word frequencies
            Dictionary<string, int> wordsFrequency = WordSignRushStatistics.wordsFrequency();

            // Sort by value in descending order and take the top 5
            var top5Words = wordsFrequency
                .OrderByDescending(pair => pair.Value)  // Sort by frequency (value)
                .Take(5)  // Take the top 5 most frequent words
                .ToDictionary(pair => pair.Key, pair => pair.Value);  // Convert to dictionary

            return top5Words;
        }

    }

}
