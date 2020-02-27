using Devdeb.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Devdeb.Tests.Network.Client
{
	class Program
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;

		static void Main(string[] args)
		{
			Console.WriteLine("Client");
			Thread.Sleep(3000);
			RunTestClient();
			Console.ReadKey();
		}

		static void RunClient()
		{
			TCPConnectionClient tcpConnection = new TCPConnectionClient(_iPAddress, _port);
			tcpConnection.Start();
		}

		static void RunTestClient()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(new IPEndPoint(_iPAddress, _port));
			socket.Blocking = true;
			Console.WriteLine("Connected");
			string message = "Hello";
			byte[] messageBytes = Encoding.ASCII.GetBytes(message);
			byte[] buffer = new byte[messageBytes.Length + 4];
			int messageLenght = messageBytes.Length;
			for (int i = 0; i < 4; i++)
			{
				buffer[i] = (byte)((messageLenght >> i * 8) & byte.MaxValue);
			}
			for (int i = 4; i < buffer.Length; i++)
			{
				buffer[i] = messageBytes[i - 4];
			}
			int count = socket.Send(buffer);
		}

	}
}
