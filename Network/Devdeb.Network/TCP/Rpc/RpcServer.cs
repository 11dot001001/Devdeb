using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Handler;
using Devdeb.Network.TCP.Rpc.RpcRequest;
using Devdeb.Serialization.Default;
using System;
using System.Net;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcServer<THandler> : BaseExpectingTcpServer
	{
		private readonly ProcessRequest<THandler>[] _requestHandlers;
		private readonly THandler _handlerInstance;

		public RpcServer(IPAddress iPAddress, int port, int backlog, THandler handler) : base(iPAddress, port, backlog)
		{
			_handlerInstance = handler ?? throw new ArgumentNullException(nameof(handler));
			_requestHandlers = new HandlerBuilder<THandler>().Build();
		}

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
		{
			byte[] buffer = new byte[count];
			tcpCommunication.Receive(buffer, 0, count);

			int offset = 0;
			RpcRequestMeta requestMeta = DefaultSerializer<RpcRequestMeta>.Instance.Deserialize(buffer, ref offset);
			_requestHandlers[requestMeta.MethodIndex](buffer, ref offset, _handlerInstance);
		}
	}
}
