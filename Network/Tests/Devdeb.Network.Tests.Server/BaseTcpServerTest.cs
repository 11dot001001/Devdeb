using Devdeb.Network.TCP;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Net;

namespace Devdeb.Network.Tests.Server
{
    public class BaseTcpServerTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;
        static private readonly int _backlog = 1;

        public void Test()
        {
            ExpectingTcpServer tcpServer = new ExpectingTcpServer(_iPAddress, _port, _backlog);
            tcpServer.Start();
            Console.ReadKey();
        }

        private class TcpServer : BaseTcpServer
        {
            public TcpServer(IPAddress iPAddress, int port, int backlog) : base(iPAddress, port, backlog)
            { }

            protected override void ProcessAccept(TcpCommunication tcpCommunication)
            {
                Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} was accepted.");
            }

            protected override void ProcessCommunication(TcpCommunication tcpCommunication)
            {
                Console.WriteLine($"Processing {tcpCommunication.Socket.RemoteEndPoint}.");
                if (tcpCommunication.BufferBytesCount != 0)
                {
                    string message = tcpCommunication.Receive(StringSerializer.UTF8, tcpCommunication.BufferBytesCount);
                    Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} message: {message}.");
                }
            }
        }

        private class ExpectingTcpServer : BaseExpectingTcpServer
        {
            public ExpectingTcpServer(IPAddress iPAddress, int port, int backlog) : base(iPAddress, port, backlog) { }

            protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
            {
                string message = tcpCommunication.Receive(StringLengthSerializer.UTF8, count);
                Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} message: {message}.");
                tcpCommunication.SendWithSize(StringLengthSerializer.UTF8, $"Server message: message <{message}> was received.");
            }
        }
    }
}
