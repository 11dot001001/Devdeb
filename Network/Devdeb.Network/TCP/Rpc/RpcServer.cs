using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Handler;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcServer<THandler, TRequestor> : BaseExpectingTcpServer
	{
		private readonly ISerializer<CommunicationMeta> _metaSerializer;
		private readonly Dictionary<TcpCommunication, RpcRequestor<TRequestor>> _connections;
		private readonly RpcHandler<THandler> _handler;

		public RpcServer(IPAddress iPAddress, int port, int backlog, THandler handler) : base(iPAddress, port, backlog)
		{
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_connections = new Dictionary<TcpCommunication, RpcRequestor<TRequestor>>();
			_handler = new RpcHandler<THandler>(handler);
		}

		protected override void ProcessAccept(TcpCommunication tcpCommunication)
		{
			base.ProcessAccept(tcpCommunication);
			_connections.Add(tcpCommunication, RpcRequestor<TRequestor>.Create(tcpCommunication));
		}

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
		{
			byte[] buffer = new byte[count];
			tcpCommunication.Receive(buffer, 0, count);

			Task.Factory.StartNew(() =>
			{
				int offset = 0;
				CommunicationMeta meta = _metaSerializer.Deserialize(buffer, ref offset);

				switch (meta.Type)
				{
					case CommunicationMeta.PackageType.Request:
						_handler.HandleRequest(tcpCommunication, meta, buffer, offset);
						break;
					case CommunicationMeta.PackageType.Response:
						{
							RpcRequestor<TRequestor> clientRequestor = _connections[tcpCommunication];
							clientRequestor.HandleResponse(meta, buffer, offset);
							break;
						}
					default: throw new Exception($"Invalid value {meta.Type} for {nameof(meta.Type)}.");
				}

			    RpcRequestor<TRequestor> clientRequestor2 = _connections[tcpCommunication];
				Test((TRequestor)(object)clientRequestor2);
			});
		}

		public Action<TRequestor> Test { get; set; }
	}
}
