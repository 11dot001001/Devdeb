using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Devdeb.Network.Tests.Client
{
	class ClientApplication
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("192.168.1.64");
		static private readonly int _port = 25000;

		static void Main(string[] args)
		{
			Console.WriteLine("Client");
			RunTestClient();
			Console.ReadKey();
		}

		static void RunTestClient()
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(new IPEndPoint(_iPAddress, _port));
			socket.Blocking = false;
			Console.WriteLine("Connected");
			byte[] buffer = new byte[100000000];
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = 1;
			}
			for (; ; )
			{
				Console.ReadKey();
				int count = socket.Send(buffer, 0, buffer.Length, SocketFlags.None, out SocketError socketError);
				if (socketError != SocketError.Success)
					Console.WriteLine($"Error: {nameof(SocketError)} is {socketError}.");
				Console.WriteLine($"The bytes were sent. Count : {count}");
				//Thread.Sleep(1000);
			}
		}
	}
}
