using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Serialization.Serializers;
using System;
using System.Net;
using System.Threading;

namespace Devdeb.Network.Tests.Server
{
    public class ExpectingTcpTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;
        static private readonly int _backlog = 1;

        public void Test()
        {
            Thread.Sleep(10000);
            ExpectingTcpServer tcpServer = new ExpectingTcpServer(_iPAddress, _port, _backlog);
            tcpServer.Start();
            Console.ReadKey();
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
