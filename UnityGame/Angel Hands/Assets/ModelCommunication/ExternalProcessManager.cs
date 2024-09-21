using Assets.Logger;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Assets.ModelCommunication
{
    public class ExternalProcessManager : MonoBehaviour
    {
        private Process externalProcess;
        private Thread processThread;
        private bool isProcessRunning = false;
        private string rootPath = Path.Combine(Application.streamingAssetsPath, "RuntimeModel");
        private string exePath = Path.Combine(Application.streamingAssetsPath, "RuntimeModel", "ModelServer.exe");
        private string arguments = "\"" +Path.Combine(Application.streamingAssetsPath, "ModelCommunication", "ActiveModel") + "\" --show_video --Threshold 0.4 --Sensitivity 2"; //--show_video

        //Start is called before the first frame update
        void Start()
        {
            StartExternalProcess();
        }

        //OnApplicationQuit is called when the application is closing
        void OnApplicationQuit()
        {
            StopExternalProcess();
        }

        public void StartExternalProcess()
        {
            try
            {
                FileLogger.Log($"root path: {rootPath}");
                if (Directory.Exists(rootPath))
                    FileLogger.Log($"Root Directory Found");
                else
                    FileLogger.Log($"Root Directory does not exist");


                if(File.Exists(exePath))
                    FileLogger.Log($"Exe File  Found");
                else
                    FileLogger.Log($"Exe File not Found");

                FileLogger.Log($"Exe path: {exePath}");
                FileLogger.Log($"arguments string: {arguments}");
                if (isProcessRunning)
                {
                    FileLogger.LogWarning("External process is already running.");
                    return;
                }

                isProcessRunning = true;
                processThread = new Thread(RunProcess);
                processThread.Start();
            }
            catch
            {
                FileLogger.LogError("Failed to start external process.");
            }
        }

        private void RunProcess()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                };

                externalProcess = new Process { StartInfo = processStartInfo };
                externalProcess.Start();

                externalProcess.WaitForExit();
            }
            catch (System.Exception ex)
            {
                FileLogger.LogError($"Failed to run external process: {ex.Message}");
            }
            finally
            {
                isProcessRunning = false;
            }
        }

        public void StopExternalProcess()
        {
            if (externalProcess != null && !externalProcess.HasExited)
            {
                try
                {
                    externalProcess.Kill();
                    externalProcess.WaitForExit(); // Wait for the process to exit
                    externalProcess.Dispose();
                }
                catch (System.Exception ex)
                {
                    FileLogger.LogError($"Failed to stop external process: {ex.Message}");
                }
                finally
                {
                    externalProcess = null;
                }
            }

            //Make sure the thread is cleaned up
            if (processThread != null && processThread.IsAlive)
            {
                processThread.Join();
            }
        }
    }
}

