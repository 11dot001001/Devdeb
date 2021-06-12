using Devdeb.Network.TCP.Communication;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Net;

namespace Devdeb.Network.TCP.Expecting
{
    public abstract class BaseExpectingTcpServer : BaseTcpServer
    {
        private readonly Dictionary<TcpCommunication, CommunicationState> _communicationsStates;

        public BaseExpectingTcpServer(IPAddress iPAddress, int port, int backlog) : base(iPAddress, port, backlog)
        {
            _communicationsStates = new Dictionary<TcpCommunication, CommunicationState>();
        }

        protected override void ProcessAccept(TcpCommunication tcpCommunication)
        {
            Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} was accepted.");
            _communicationsStates.Add(tcpCommunication, new CommunicationState());
        }

        protected sealed override void ProcessCommunication(TcpCommunication tcpCommunication)
        {
            CommunicationState communicationState = _communicationsStates[tcpCommunication];
            if (tcpCommunication.BufferBytesCount < communicationState.ExpectingBytesCount)
                return;

            if (!communicationState.IsLengthReceived)
            {
                communicationState.ExpectingBytesCount = tcpCommunication.Receive(Int32Serializer.Default);
                communicationState.IsLengthReceived = true;
                if (tcpCommunication.BufferBytesCount < communicationState.ExpectingBytesCount)
                    return;
            }
            ProcessCommunication(tcpCommunication, communicationState.ExpectingBytesCount);
            _communicationsStates[tcpCommunication] = new CommunicationState();
        }
        protected abstract void ProcessCommunication(TcpCommunication tcpCommunication, int count);
    }
}
