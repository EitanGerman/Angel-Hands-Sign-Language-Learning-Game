using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Assets.Scripts.GameManager;
using Unity.VisualScripting;
using System.Threading.Tasks;

namespace Assets.ModelCommunication
{

    public class ClientController : MonoBehaviour
    {
        private TcpClient socketConnection;
        private Thread clientReceiveThread;
        private string serverAddress = "127.0.0.1"; // Replace with your server IP address
        private int serverPort = 65431; // Replace with your server port

        void Start()
        {
            //ConnectToServer();
            Task.Run(async () => await ConnectToServerAsync());
        }
        private async Task ConnectToServerAsync()
        {
            while (true)
            {
                try
                {
                    if (socketConnection == null || !socketConnection.Connected)
                    {
                        socketConnection = new TcpClient();
                        await socketConnection.ConnectAsync(serverAddress, serverPort);
                        Debug.Log("Connected to server successfully.");
                        StartListening();
                        break; // Exit loop on successful connection
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Socket connection error: {e.Message} (Model not running). Retrying in 1 second...");
                }

                // Wait for 1 second before retrying
                await Task.Delay(1000);
            }
        }

        private void ConnectToServer()
        {

            try
            {
                socketConnection = new TcpClient(serverAddress, serverPort);
                StartListening();
            }
            catch (Exception e)
            {
                Debug.LogError($"Socket connection error: {e.Message} (Model not running)");
            }
        }

        private void StartListening()
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }

        private void ListenForData()
        {
            try
            {
                byte[] bytes = new byte[1024];
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    while (true)
                    {
                        int bytesRead = stream.Read(bytes, 0, bytes.Length);
                        if (bytesRead > 0)
                        {
                            string message = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                            Debug.Log($"Received from server: {message}");
                            GameManager.Instance?.SetCurrentWord(message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Socket read error: {e.Message}");
            }
        }

        void OnApplicationQuit()
        {
            clientReceiveThread.Abort();
            if (socketConnection != null)
            {
                socketConnection.Close();
            }
        }

    }

}
