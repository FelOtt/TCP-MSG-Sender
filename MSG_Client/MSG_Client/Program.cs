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
            Console.Write("Enter server IP address or domain: ");
            string serverAddress = Console.ReadLine();

            Console.Write("Enter your nickname: ");
            nickname = Console.ReadLine();

            try
            {
                client = new TcpClient(serverAddress, 8888);
                clientStream = client.GetStream();

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                receiveThread.Start();

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
                clientStream.Close();
                client.Close();
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
