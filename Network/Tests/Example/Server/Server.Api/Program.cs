using Devdeb.Network.TCP.Rpc;
using System.Net;

namespace Server.App
{
	class Program
	{
		static private readonly IPAddress _iPAddress = IPAddress.Parse("127.0.0.1");
		static private readonly int _port = 25000;
		static private readonly int _backlog = 1;

		static void Main(string[] args)
		{
			RpcServer server = new RpcServer(_iPAddress, _port, _backlog, new Startup());
			server.Start();
		}
	}
}
