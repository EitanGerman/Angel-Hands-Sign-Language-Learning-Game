using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * recive data from port
 port: number is set in variable named source
 output: data is stored in variable named data
 */
namespace Assets.Scripts.Utills.Communication.Port
{
    public class PortListener : Communication
    {
        
        private Thread listenerThread;
        private bool isRunning = true;
        private TcpListener listener;
        private string receivedData;

        public override string source { get; set; }
        public int port; // Port number to listen on
        public override string data { get; set; }

        public override void StartCommunication()
        {
            // Start the listener thread
            port = int.Parse(source);
            listenerThread = new Thread(ListenForData);
            listenerThread.Start();
        }

        public override void Destroy()
        {
            // Stop the listener thread when the script is destroyed
            isRunning = false;
            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join();
            }
    
            // Stop the listener
            if (listener != null)
            {
                listener.Stop();
            }
        }
    
        private void ListenForData()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Debug.Log("Listening on port " + port);
    
            while (isRunning)
            {
                if (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string port_data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    
                    // Update the received data in a thread-safe manner
                    lock (this)
                    {
                        receivedData = port_data;
                        data = port_data;
                        Debug.Log("Received data: " + data);
                    }
    
                    client.Close();
                }
    
                // Sleep for a short time to prevent high CPU usage
                Thread.Sleep(100);
            }
        }
    }
}
