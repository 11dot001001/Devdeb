using Devdeb.Network.TCP.Communication;
using System;
using System.Net;
using System.Net.Sockets;

namespace Devdeb.Network.Tests.Server
{
    public class TcpCommunicationTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;
        static private readonly int _backlog = 1;

        public void Test()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(_iPAddress, _port));
            socket.Listen(_backlog);
            Socket acceptedSocket = socket.Accept();
            TcpCommunication acceptedCommunication = new TcpCommunication(acceptedSocket);

            long totalReceived = 0;
            byte[] buffer = new byte[30000];

            for (; ; )
            {
                acceptedCommunication.ReceiveToBuffer();
                Console.WriteLine("Received bytes count: " + acceptedCommunication.ReceivedBytesCount);

                if (acceptedCommunication.ReceivedBytesCount != 0)
                {
                    int receivedBytesCount = acceptedCommunication.ReceivedBytesCount;
                    int readCount = receivedBytesCount <= buffer.Length ? receivedBytesCount : buffer.Length;
                    acceptedCommunication.Receive(buffer, 0, readCount);
                    totalReceived += readCount;
                }
                Console.WriteLine("Total received: " + totalReceived);
            }
        }
    }
}
