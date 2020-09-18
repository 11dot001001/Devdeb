using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Devdeb.Tests.Network.Server
{
	class ServerApplication
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("192.168.1.64");
		static private readonly int _port = 25000;
		static private readonly int _backlog = 1;

		static void Main(string[] args)
		{
			Console.WriteLine("Server");
			RunTestServer();
			Console.ReadKey();
		}

		static void RunTestServer()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(new IPEndPoint(_iPAddress, _port));
			socket.Listen(_backlog);
			Socket acceptedSocket = socket.Accept();
			acceptedSocket.Blocking = true;
			acceptedSocket.ReceiveBufferSize = 1000;
			Console.WriteLine($"ReceiveBufferSize: {acceptedSocket.ReceiveBufferSize}");
			for (; ; )
			{
				byte[] buffer = new byte[100000000];
				int receivedBytesCount = 0;
				if (acceptedSocket.Available != 0)
				{
					Console.WriteLine(acceptedSocket.Available);
					receivedBytesCount = acceptedSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None, out SocketError socketError);
					if (socketError != SocketError.Success)
						Console.WriteLine($"Error: {nameof(SocketError)} is {socketError}.");
				}
				Console.WriteLine($"Bytes were received. Count : {receivedBytesCount}. Available : {acceptedSocket.Available}");
				Thread.Sleep(1000);
			}
		}
	}
}