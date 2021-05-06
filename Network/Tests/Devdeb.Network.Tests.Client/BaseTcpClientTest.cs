using Devdeb.Network.TCP;
using Devdeb.Network.TCP.Connection;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Net;

namespace Devdeb.Network.Tests.Client
{
    public class BaseTcpClientTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;

        public void Test()
        {
            TcpClient tcpClient = new TcpClient(_iPAddress, _port);
            tcpClient.Start();
            for (; ; )
            {
                string message = Console.ReadLine();

                byte[] buffer = new byte[StringLengthSerializer.UTF8.Size(message)];
                StringLengthSerializer.UTF8.Serialize(message, buffer, 0);

                byte[] bufferLength = new byte[Int32Serializer.Default.Size];
                Int32Serializer.Default.Serialize(buffer.Length, bufferLength, 0);

                tcpClient.Send(bufferLength, 0, bufferLength.Length);
                tcpClient.Send(buffer, 0, buffer.Length);
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
    }
}
