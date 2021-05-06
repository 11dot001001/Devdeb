using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Network.Tests.Server
{
    public class DefaultTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;
        static private readonly int _backlog = 1;

        public void Test()
        {
            byte[] buffer2 = new byte[] { 0, 1, 1, 1 };
            Array.Copy(buffer2, 1, buffer2, 0, 2);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(_iPAddress, _port));
            socket.Listen(_backlog);
            Socket acceptedSocket = socket.Accept();
            acceptedSocket.Blocking = false;
            Console.WriteLine($"ReceiveBufferSize: {acceptedSocket.ReceiveBufferSize}");
            for (; ; )
            {
                byte[] buffer = new byte[300000000];
                int receivedBytesCount = 0;
                if (acceptedSocket.Available != 0)
                {
                    Console.WriteLine(acceptedSocket.Available);
                    receivedBytesCount = acceptedSocket.Receive(buffer, 0, acceptedSocket.Available, SocketFlags.None, out SocketError socketError);
                    if (socketError != SocketError.Success)
                        Console.WriteLine($"Error: {nameof(SocketError)} is {socketError}.");
                }
                Console.WriteLine($"Bytes were received. Count : {receivedBytesCount}. Available : {acceptedSocket.Available}");
                Thread.Sleep(1000);
            }
        }
    }
}
