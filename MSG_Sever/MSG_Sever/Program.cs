using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MSG_Sever
{
    internal class Program
    {
        static TcpListener server;
        static List<TcpClient> clients = new List<TcpClient>();
        static object lockObject = new object();

        static void Main(string[] args)
        {
            StartServer();
            Thread listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            Console.Title = "Msg_Server by darthmaus";
            Console.WriteLine("Press 'Q' to shut down the server.");
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Q)
                {
                    break;
                }
            }

            CloseServer();
        }

        static void StartServer()
        {
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();
            Console.WriteLine("Server started on port 8888");
        }

        static void CloseServer()
        {
            server.Stop();
            lock (lockObject)
            {
                foreach (TcpClient client in clients)
                {
                    client.Close();
                }
            }
            Console.WriteLine("Server closed.");
            Environment.Exit(0);
        }

        static void ListenForClients()
        {
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                lock (lockObject)
                {
                    clients.Add(client);
                }

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }

        static void HandleClient(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                    break;

                string receivedMessage = Encoding.UTF8.GetString(message, 0, bytesRead);

                // Check if the received message is not empty before processing and broadcasting
                if (!string.IsNullOrWhiteSpace(receivedMessage))
                {
                    LogMessage($"Received: {receivedMessage}");
                    BroadcastMessage(receivedMessage);
                }
            }

            lock (lockObject)
            {
                clients.Remove(tcpClient);
            }

            tcpClient.Close();
        }

        static void BroadcastMessage(string message)
        {
            lock (lockObject)
            {
                foreach (TcpClient client in clients)
                {
                    NetworkStream clientStream = client.GetStream();
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                }
            }
        }

        static void LogMessage(string log)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {log}");
        }
    }
}
