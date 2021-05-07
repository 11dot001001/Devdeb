using Devdeb.Network.TCP;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Serialization.Serializers;
using System;
using System.Net;
using System.Threading;

namespace Devdeb.Network.Tests.Client
{
    public class BaseTcpClientTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;

        public void Test()
        {
            Thread.Sleep(1000); //Fix it
            ExpectingTcpClient tcpClient = new ExpectingTcpClient(_iPAddress, _port);
            tcpClient.Start();
            for (; ; )
            {
                string message = Console.ReadLine();
                tcpClient.SendWithSize(StringLengthSerializer.UTF8, message);

                Console.WriteLine("Message sent.");
            }
        }

        private class TcpClient : BaseTcpClient
        {
            public TcpClient(IPAddress serverIPAddress, int serverPort, int maxConnectionAttempts = 4)
                : base(serverIPAddress, serverPort, maxConnectionAttempts)
            { }

            protected override void ProcessCommunication(TcpCommunication tcpCommunication)
            {
            }
        }

        private class ExpectingTcpClient : BaseExpectingTcpClient
        {
            public ExpectingTcpClient(IPAddress iPAddress, int port) : base(iPAddress, port) { }

            protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
            {
                string message = tcpCommunication.Receive(StringLengthSerializer.UTF8, count);
                Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} message: {message}.");
            }
        }
    }
}
