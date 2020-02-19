using Devdeb.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Tests.Network.Server
{
	class Program
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;
		static private readonly int _backlog = 5;

		static void Main(string[] args)
		{
			Console.WriteLine("Server");
			RunServer();
			Console.ReadKey();
		}

		static void RunServer()
		{
			TCPServer tcpServer = new TCPServer(_iPAddress, _port, _backlog);
			tcpServer.Start();
		}

		static void RunTestServer()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(new IPEndPoint(_iPAddress, _port));
			socket.Listen(_backlog);
			Socket acceptedSocket = socket.Accept();
			List<byte[]> buffers = new List<byte[]>();
			for (; ; )
			{
				if (acceptedSocket.Available != 0)
				{
					try
					{
						byte[] buffer = new byte[acceptedSocket.ReceiveBufferSize];
						acceptedSocket.Receive(buffer);
						buffers.Add(buffer);
					}
					catch (Exception e)
					{

					}
				}
				Thread.Sleep(1);
			}
		}
	}
}