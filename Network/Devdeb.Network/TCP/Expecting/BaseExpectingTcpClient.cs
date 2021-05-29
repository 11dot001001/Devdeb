using Devdeb.Network.TCP.Communication;
using Devdeb.Serialization.Serializers.System;
using System.Net;

namespace Devdeb.Network.TCP.Expecting
{
    public abstract class BaseExpectingTcpClient : BaseTcpClient
    {
        private CommunicationState _communicationState;

        public BaseExpectingTcpClient(IPAddress serverIPAddress, int serverPort, int maxConnectionAttempts = 4)
            : base(serverIPAddress, serverPort, maxConnectionAttempts)
        {
            _communicationState = new CommunicationState();
        }

        protected override void ProcessCommunication(TcpCommunication tcpCommunication)
        {
            if (tcpCommunication.BufferBytesCount < _communicationState.ExpectingBytesCount)
                return;

            if (!_communicationState.IsLengthReceived)
            {
                _communicationState.ExpectingBytesCount = tcpCommunication.Receive(Int32Serializer.Default);
                _communicationState.IsLengthReceived = true;
                if (tcpCommunication.BufferBytesCount < _communicationState.ExpectingBytesCount)
                    return;
            }
            ProcessCommunication(tcpCommunication, _communicationState.ExpectingBytesCount);
            _communicationState = new CommunicationState();
        }

        protected abstract void ProcessCommunication(TcpCommunication tcpCommunication, int count);
    }
}
