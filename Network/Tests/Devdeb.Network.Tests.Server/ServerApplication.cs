using Devdeb.Network.TCP;
using Devdeb.Network.TCP.Connection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Devdeb.Tests.Network.Server
{
	class ServerApplication
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
			Server server = new Server(_iPAddress, _port, _backlog);
			server.Start();
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
	public class Server : BaseTCPServer
	{
		public Server(IPAddress ipAddress, int port, int backlog) : base(ipAddress, port, backlog) { }

		protected override void ReceiveBytes(TCPConnectionProvider connectionProvider, byte[] buffer)
		{
			string recivedMessage = Encoding.UTF8.GetString(buffer);
			Console.WriteLine($"Received message from {connectionProvider.Connection.RemoteEndPoint}: {recivedMessage}");
			SendBytes(connectionProvider, Encoding.UTF8.GetBytes($"Server received your message: {recivedMessage}"));
		}
	}
}