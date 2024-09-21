using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using Assets.Scripts.Utills.Communication;

/*
 * recive data from a file
 file_path: path is set in variable named source
 output: data is stored in variable named data
 */
namespace Assets.Scripts.Utills.Communication.Files
{
    public class FileReader :  Communication
    {
        public override string source {get;set; }// Name of the file path
        public override string data { get; set; }
        private string fileContents; // Variable to store the file contents
        public float checkInterval = 1.0f; // Interval in seconds to check the file
        private Thread fileReaderThread;
        private bool isRunning = true;

        public override void StartCommunication()
        {
            // Start the file reader thread
            fileReaderThread = new Thread(ReadFile);
            fileReaderThread.Start();
        }

        public override void Destroy()
        {
            // Stop the file reader thread when the script is destroyed
            fileReaderThread.Abort();
        }

        private void ReadFile()
        {
            while (isRunning)
            {
                if (File.Exists(source))
                {
                    // Read the file contents
                    string contents = File.ReadAllText(source);

                    // Update the file contents in a thread-safe manner
                    lock (this)
                    {
                        fileContents = contents;
                        data = contents;
                    }
                    Debug.Log("file content :" + fileContents);
                }
                else
                {
                    Debug.LogWarning("File not found: " + source);
                }

                // Wait for the specified interval before checking again
                Thread.Sleep((int)(checkInterval * 1000)); // milliseconds
            }//while
            Destroy();
        }//ReadFile
    }//FileReader
}//namespace
