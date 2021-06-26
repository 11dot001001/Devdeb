using Devdeb.Network.TCP.Rpc;
using System;
using System.Net;
using System.Text;

namespace Client.App
{
	class Program
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;

		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			RpcClient client = new RpcClient(_iPAddress, _port, new Startup());
			client.Start();
		}
	}
}
