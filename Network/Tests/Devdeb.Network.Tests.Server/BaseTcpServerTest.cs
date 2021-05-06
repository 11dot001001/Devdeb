using Devdeb.Network.TCP;
using Devdeb.Network.TCP.Connection;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Devdeb.Network.Tests.Server
{
    public class BaseTcpServerTest
    {
        static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
        static private readonly int _port = 25000;
        static private readonly int _backlog = 1;

        public void Test()
        {
            TcpServer tcpServer = new TcpServer(_iPAddress, _port, _backlog);
            tcpServer.Start();
            Console.ReadKey();
        }

        private class TcpServer : BaseTcpServer
        {
            private readonly Dictionary<TcpCommunication, CommunicationState> _communicationsStates;

            public TcpServer(IPAddress iPAddress, int port, int backlog) : base(iPAddress, port, backlog)
            {
                _communicationsStates = new Dictionary<TcpCommunication, CommunicationState>();
            }

            protected override void ProcessAccept(TcpCommunication tcpCommunication)
            {
                Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} was accepted.");
                _communicationsStates.Add(tcpCommunication, new CommunicationState());
            }

            protected override void ProcessCommunication(TcpCommunication tcpCommunication)
            {
                //Console.WriteLine($"Processing {tcpCommunication.Socket.RemoteEndPoint}.");

                CommunicationState communicationState = _communicationsStates[tcpCommunication];
                if (tcpCommunication.ReceivedBytesCount < communicationState.ExpectingBytesCount)
                    return;

                if (!communicationState.IsLengthReceived)
                {
                    byte[] lengthBuffer = new byte[Int32Serializer.Default.Size];
                    tcpCommunication.Receive(lengthBuffer, 0, lengthBuffer.Length);
                    communicationState.ExpectingBytesCount = Int32Serializer.Default.Deserialize(lengthBuffer, 0);

                    if (tcpCommunication.ReceivedBytesCount < communicationState.ExpectingBytesCount)
                        return;
                }

                byte[] buffer = new byte[communicationState.ExpectingBytesCount];
                tcpCommunication.Receive(buffer, 0, buffer.Length);

                string message = StringLengthSerializer.UTF8.Deserialize(buffer, 0, buffer.Length);
                Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} message: {message}.");
                _communicationsStates[tcpCommunication] = new CommunicationState();
            }
        }

        public class CommunicationState
        {
            public int ExpectingBytesCount { get; set; }

            public bool IsLengthReceived { get; set; }

            public CommunicationState()
            {
                ExpectingBytesCount = Int32Serializer.Default.Size;
            }
        }
    }
}
