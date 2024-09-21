using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Assets.Scripts.Utills.Communication;


/*
 recive data from a environment variable
 environment variable: is set in variable named source
 output: data is stored in variable named data
 */
namespace Assets.Scripts.Utills.Communication.EnvVar
{
    public class EnvironmentVariableWatcher : Communication
    {

        public override string source {get;set;}
        public override string data { get; set; }
        private string environmentVariableValue; // Variable to store the environment variable value
        public float checkInterval = 1.0f; // Interval in seconds to check the environment variable
        private Thread environmentVariableWatcherThread;
        private bool isRunning = true;
        private ConcurrentQueue<System.Action> mainThreadActions = new ConcurrentQueue<System.Action>();

        public override void StartCommunication()
        {
            // Start the environment variable watcher thread
            environmentVariableWatcherThread = new Thread(CheckEnvironmentVariable);
            environmentVariableWatcherThread.Start();
        }

        public override void Destroy()
        {
            environmentVariableWatcherThread.Abort();
        }

        private void CheckEnvironmentVariable()
        {
            while (isRunning)
            {
                // Read the environment variable value
                string value = System.Environment.GetEnvironmentVariable(source);
                Debug.Log("Checking environment variable: " + source);
                Debug.Log("Value retrieved: " + value);
                if (value != null)
                {
                    // Update the environment variable value in a thread-safe manner
                    lock (this)
                    {
                        environmentVariableValue = value;
                        data= value;
                    }

                    // Queue an action to log the value on the main thread
                    //mainThreadActions.Enqueue(() => Debug.Log("Environment variable updated: " + value));
                    Debug.Log("Environment variable updated: " + value);
                }
                else
                {
                    // Queue a warning log on the main thread
                    //mainThreadActions.Enqueue(() => Debug.LogWarning("Environment variable not found: " + source));
                    Debug.LogWarning("Environment variable not found: " + source);
                }

                // Wait for the specified interval before checking again
                Thread.Sleep((int)(checkInterval * 1000)); // milliseconds
            }
        }

        void Update()
        {
            // Process actions queued for the main thread
            while (mainThreadActions.TryDequeue(out var action))
            {
                action();
            }

            // Use the environment variable value in your game logic
            // For example, you can display it in the UI or use it in other scripts
            Debug.Log("Current environment variable value: " + environmentVariableValue);
        }
    }
}
