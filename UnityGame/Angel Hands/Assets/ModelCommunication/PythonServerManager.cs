using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Assets.ModelCommunicaiotn
{

    public class PythonServerManager : MonoBehaviour
    {
        private Process pythonProcess;
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private Thread clientThread;

        void Start()
        {
            StartPythonServer();
            ConnectToServer();
        }

        void StartPythonServer()
        {
            try
            {
                pythonProcess = new Process();
                pythonProcess.StartInfo.FileName = "python"; // Path to Python executable
                pythonProcess.StartInfo.Arguments = "path_to_your_python_script.py"; // Path to your script
                pythonProcess.StartInfo.UseShellExecute = false;
                pythonProcess.StartInfo.RedirectStandardOutput = true;
                pythonProcess.StartInfo.RedirectStandardError = true;
                pythonProcess.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data);
                pythonProcess.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data);
                pythonProcess.Start();
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();

                UnityEngine.Debug.Log("Started Python server");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Failed to start Python server: " + e.Message);
            }
        }

        void ConnectToServer()
        {
            clientThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    client = new TcpClient("localhost", 65432);
                    reader = new StreamReader(client.GetStream());
                    writer = new StreamWriter(client.GetStream());
                    UnityEngine.Debug.Log("Connected to Python server");

                    while (true)
                    {
                        writer.WriteLine("get");
                        writer.Flush();

                        string serverMessage = reader.ReadLine();
                        UnityEngine.Debug.Log("Received from server: " + serverMessage);

                        // Handle the received data (e.g., update Unity UI or other variables)
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error: " + e.Message);
                }
            }));
            clientThread.Start();
        }

        void OnApplicationQuit()
        {
            if (client != null)
            {
                writer.WriteLine("exit");
                writer.Flush();
                reader.Close();
                writer.Close();
                client.Close();
            }

            if (clientThread != null && clientThread.IsAlive)
            {
                clientThread.Abort();
            }

            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
            }
        }
    }
}