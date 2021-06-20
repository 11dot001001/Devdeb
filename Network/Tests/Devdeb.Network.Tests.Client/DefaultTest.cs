using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Devdeb.Network.Tests.Client
{
    public class DefaultTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;

        public void Test()
        {
            Thread.Sleep(2000);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(_iPAddress, _port));
            socket.Blocking = true;
            Console.WriteLine("Connected");
            Console.WriteLine($"LocalEndPoint: {socket.LocalEndPoint}");

            int totalSent = 0;
            byte[] buffer = new byte[30000];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 1;

            for (; ; )
            {
                if (Console.ReadKey().Key == ConsoleKey.A)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    break;
                }

                int count = socket.Send(buffer, 0, buffer.Length, SocketFlags.None, out SocketError socketError);
                totalSent += count;
                if (socketError != SocketError.Success)
                    Console.WriteLine($"Error: {nameof(SocketError)} is {socketError}.");
                Console.WriteLine($"The bytes were sent. Count : {count}");
                Console.WriteLine($"Total sent : {totalSent}");
                //Thread.Sleep(1000);
            }
        }
    }
}
