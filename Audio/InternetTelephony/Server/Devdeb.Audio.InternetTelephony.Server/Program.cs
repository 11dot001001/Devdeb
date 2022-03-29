using Devdeb.Network.TCP.Rpc;
using System;
using System.Net;
using System.Text;

namespace Devdeb.Audio.InternetTelephony.Server
{
	internal class Program
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("192.168.1.66");
		//static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;
		static private readonly int _backlog = 1;

		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			RpcServer server = new(_iPAddress, _port, _backlog, new Startup());
			server.Start();
		}
	}
}
