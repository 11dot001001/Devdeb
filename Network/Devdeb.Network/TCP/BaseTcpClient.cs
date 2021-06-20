using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using Devdeb.Serialization;
using Devdeb.Network.TCP.Communication;

namespace Devdeb.Network.TCP
{
    public abstract class BaseTcpClient : IDisposable
    {
        private readonly Thread _connectionProcessing;
        private readonly IPAddress _serverIPAddress;
        private readonly int _serverPort;
        private readonly int _maxConnectionAttempts;
        private TcpCommunication _tcpCommunication;
        private bool _isStarted;

        public BaseTcpClient(IPAddress serverIPAddress, int serverPort, int maxConnectionAttempts = 4)
        {
            _serverIPAddress = serverIPAddress ?? throw new ArgumentNullException(nameof(serverIPAddress));
            _serverPort = serverPort;
            _maxConnectionAttempts = maxConnectionAttempts;
            _connectionProcessing = new Thread(ProcessCommunication);
        }

        public int ReceivedBytesCount => _tcpCommunication.ReceivedBytesCount;
        protected TcpCommunication TcpCommunication => _tcpCommunication;

        public void Receive(byte[] buffer, int offset, int count)
        {
            VerifyClientState();
            _tcpCommunication.Receive(buffer, offset, count);
        }
        public void Receive(byte[] buffer, ref int offset, int count)
        {
            Receive(buffer, offset, count);
            offset += count;
        }
        public void Send(byte[] buffer, int offset, int count)
        {
            VerifyClientState();
            _tcpCommunication.Send(buffer, offset, count);
        }
        public void Send(byte[] buffer, ref int offset, int count)
        {
            Send(buffer, offset, count);
            offset += count;
        }
        public void Send<T>(ISerializer<T> serializer, T instance)
        {
            VerifyClientState();
            _tcpCommunication.Send(serializer, instance);
        }
        public void SendWithSize(byte[] buffer, int offset, int count)
        {
            VerifyClientState();
            _tcpCommunication.SendWithSize(buffer, offset, count);
        }
        public void SendWithSize<T>(ISerializer<T> serializer, T instance)
        {
            VerifyClientState();
            _tcpCommunication.SendWithSize(serializer, instance);
        }

		public virtual void Start()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bool connected = false;
            Exception[] exceptions = new Exception[_maxConnectionAttempts];
            for (int connectionAttemptNumber = 0; !connected; connectionAttemptNumber++)
            {
                if (connectionAttemptNumber == _maxConnectionAttempts)
                    throw new AggregateException("The connection attempts exceeds max available count.", exceptions);
                try
                {
                    socket.Connect(new IPEndPoint(_serverIPAddress, _serverPort));
                    connected = true;
                }
                catch (SocketException socketException)
                {
                    exceptions[connectionAttemptNumber] = socketException;
                    if (socketException.SocketErrorCode != SocketError.ConnectionRefused)
                        throw socketException;
                    Console.WriteLine($"Сonnection attempt {connectionAttemptNumber}. " + socketException.Message);
                }
            }

            _tcpCommunication = new TcpCommunication(socket);
            _connectionProcessing.Start();
            _isStarted = true;
            Console.WriteLine("Client has been started.");
        }
        public virtual void Stop()
        {
            _connectionProcessing.Abort();
            _tcpCommunication.Close();
        }

        protected abstract void ProcessCommunication(TcpCommunication tcpCommunication);
        protected abstract void Disconnected();

        private void VerifyClientState()
        {
            if (!_isStarted)
                throw new Exception("The client wasn't started.");
        }
        private void ProcessCommunication()
        {
            for (; ; )
            {
                if (_tcpCommunication.IsShutdown || _tcpCommunication.IsClosed)
                {
                    _tcpCommunication.Dispose();
                    Disconnected();
                    break;
                }

                _tcpCommunication.SendBuffer();
                _tcpCommunication.ReceiveToBuffer();
                ProcessCommunication(_tcpCommunication);
                Thread.Sleep(1);
            }
        }

		public void Dispose() => _tcpCommunication.Dispose();
	}
}
