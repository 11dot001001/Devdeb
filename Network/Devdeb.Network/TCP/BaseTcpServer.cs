using Devdeb.Network.TCP.Communication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Network.TCP
{
    public abstract class BaseTcpServer
    {
        private readonly Thread _acceptingThread;
        private readonly Thread _connectionProcessing;
        private readonly IPAddress _iPAddress;
        private readonly int _port;
        private readonly int _backlog;
        private readonly Socket _tcpListener;
        private readonly Queue<TcpCommunication> _tcpCommunications;
        private bool _isStarted;

        public BaseTcpServer(IPAddress iPAddress, int port, int backlog)
        {
            _iPAddress = iPAddress ?? throw new ArgumentNullException(nameof(iPAddress));
            _port = port;
            _backlog = backlog;
            _tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpListener.Bind(new IPEndPoint(_iPAddress, _port));
            _acceptingThread = new Thread(Accept);
            _connectionProcessing = new Thread(ProcessCommunication);
            _tcpCommunications = new Queue<TcpCommunication>();
        }

        public void Start()
        {
            _tcpListener.Listen(_backlog);
            _acceptingThread.Start();
            _connectionProcessing.Start();
            _isStarted = true;
            Console.WriteLine("Server has been started.");
        }

        protected abstract void ProcessCommunication(TcpCommunication tcpCommunication);
        protected abstract void ProcessAccept(TcpCommunication tcpCommunication);

        private void Accept()
        {
            for (; ; )
            {
                Socket acceptedSocket = _tcpListener.Accept();
                TcpCommunication acceptedCommunication = new TcpCommunication(acceptedSocket);
                ProcessAccept(acceptedCommunication);
                lock (_tcpCommunications)
                    _tcpCommunications.Enqueue(acceptedCommunication);
            }
        }
        private void ProcessCommunication()
        {
            for (; ; )
            {
                if (_tcpCommunications.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                TcpCommunication tcpCommunication;
                lock (_tcpCommunications)
                {
                    if (_tcpCommunications.Count == 0)
                        continue;
                    tcpCommunication = _tcpCommunications.Dequeue();
                }

                tcpCommunication.SendBuffer();
                tcpCommunication.ReceiveToBuffer();

                ProcessCommunication(tcpCommunication);

                lock (_tcpCommunications)
                    _tcpCommunications.Enqueue(tcpCommunication);

                Thread.Sleep(1);
            }
        }
    }
}
