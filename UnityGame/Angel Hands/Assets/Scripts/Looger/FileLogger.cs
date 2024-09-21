using UnityEngine;
using System.IO;

namespace Assets.Logger
{
    public static class FileLogger
    {
        private static string logFilePath;

        // This property ensures that logFilePath is initialized lazily
        private static string LogFilePath
        {
            get
            {
                if (logFilePath == null)
                {
                    Initialize();
                }
                return logFilePath;
            }
        }

        // Initialize is called lazily when LogFilePath is accessed for the first time
        private static void Initialize()
        {
            logFilePath = Path.Combine(Application.persistentDataPath, "UnityLog.txt");
            Debug.Log("Log file path: " + logFilePath);

            // Ensure the Logs directory exists
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            AppendToFile("Logger Initialized");
        }

        public static void Log(string message)
        {
            Debug.Log(message);
            AppendToFile("LOG: " + message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
            AppendToFile("WARNING: " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError(message);
            AppendToFile("ERROR: " + message);
        }

        private static void AppendToFile(string message)
        {
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine($"{System.DateTime.Now}: {message}");
            }
        }
    }
}
