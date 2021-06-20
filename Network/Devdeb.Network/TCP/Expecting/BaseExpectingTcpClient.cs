using Devdeb.Network.TCP.Communication;
using Devdeb.Serialization.Serializers.System;
using System.Net;

namespace Devdeb.Network.TCP.Expecting
{
    public abstract class BaseExpectingTcpClient : BaseTcpClient
    {
        private CommunicationState _communicationState;
        private TcpCommunication _tcpCommunication;

        public BaseExpectingTcpClient(IPAddress serverIPAddress, int serverPort, int maxConnectionAttempts = 4)
            : base(serverIPAddress, serverPort, maxConnectionAttempts)
        {
            _communicationState = new CommunicationState();
        }

        protected override void Connected(TcpCommunication tcpCommunication) => _tcpCommunication = tcpCommunication;

        protected sealed override void ProcessCommunication()
        {
            if (_tcpCommunication.BufferBytesCount < _communicationState.ExpectingBytesCount)
                return;

            if (!_communicationState.IsLengthReceived)
            {
                _communicationState.ExpectingBytesCount = _tcpCommunication.Receive(Int32Serializer.Default);
                _communicationState.IsLengthReceived = true;
                if (_tcpCommunication.BufferBytesCount < _communicationState.ExpectingBytesCount)
                    return;
            }
            ProcessCommunication(_communicationState.ExpectingBytesCount);
            _communicationState = new CommunicationState();
        }

        protected abstract void ProcessCommunication(int receivedCount);
    }
}
