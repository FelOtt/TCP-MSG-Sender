using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MSG_Client
{
    internal class Program
    {
        static TcpClient client;
        static NetworkStream clientStream;
        static string nickname;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter server IP address or domain + port [Format: example.com:1234 or 1.2.3.4:1234], or write exit to close: ");
                
                // Split the input to get the server address and port
                string serverAddressInput = Console.ReadLine();
                string serverAddress = serverAddressInput.Split(':')[0];
                int serverPort = Convert.ToInt32(serverAddressInput.Split(':')[1]);

                if (serverAddress == "exit")
                {
                    break;
                }

                Console.Write("Enter your nickname: ");
                nickname = Console.ReadLine();

                try
                {
                    client = new TcpClient(serverAddress, serverPort);
                    clientStream = client.GetStream();

                    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                    receiveThread.Start();
                    Console.WriteLine("Connected to the server.");

                    while (true)
                    {
                        string message = Console.ReadLine();
                        SendMessage($"{nickname}:\n{message}\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    try
                    {
                        if (clientStream != null)
                        {
                            clientStream.Close();
                        }
                    }
                    catch
                    {
                        throw new Exception("Error: Unable to close the network stream.");
                    }
                    finally
                    {
                        if (client != null)
                        {
                            client.Close();
                        }
                    }
                }
                Console.Clear();
            }
        }

        static void ReceiveMessages()
        {
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

                // Check if the message is from the server, and avoid local echo
                if (!receivedMessage.StartsWith($"{nickname}:"))
                {
                    Console.WriteLine($"{receivedMessage}");
                }
            }
        }

        static void SendMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }
    }
}
