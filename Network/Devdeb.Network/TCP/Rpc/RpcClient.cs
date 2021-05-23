using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Serialization.Serializers;
using System;
using System.Net;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcClient<THandler, TRequestor> : BaseExpectingTcpClient
	{
		public RpcClient(IPAddress iPAddress, int port) : base(iPAddress, port) { }

		protected override void NotifyStarted() => Requestor = RpcRequestor<TRequestor>.Create(TcpCommunication);

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
		{
			string message = tcpCommunication.Receive(StringLengthSerializer.UTF8, count);
			Console.WriteLine($"{tcpCommunication.Socket.RemoteEndPoint} message: {message}.");
		}

		public TRequestor Requestor { get; set; }
	}
}
