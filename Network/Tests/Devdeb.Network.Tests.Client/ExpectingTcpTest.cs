using Devdeb.Network.TCP.Expecting;
using Devdeb.Serialization.Serializers;
using System;
using System.Net;

namespace Devdeb.Network.Tests.Client
{
    public class ExpectingTcpTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;

        public void Test()
        {
            ExpectingTcpClient tcpClient = new ExpectingTcpClient(_iPAddress, _port);
            tcpClient.Start();
            for (; ; )
            {
                string message = Console.ReadLine();
                tcpClient.SendWithSize(StringLengthSerializer.UTF8, message);

                Console.WriteLine("Message sent.");
            }
        }

        private class ExpectingTcpClient : BaseExpectingTcpClient
        {
            public ExpectingTcpClient(IPAddress iPAddress, int port) : base(iPAddress, port) { }

            protected override void Disconnected()
            {
                throw new NotImplementedException();
            }

            protected override void ProcessCommunication(int receivedCount)
            {
            }
        }
    }
}
