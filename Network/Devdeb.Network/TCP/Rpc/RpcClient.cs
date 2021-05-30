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
	public sealed class RpcClient : BaseExpectingTcpClient
	{
		private readonly ISerializer<CommunicationMeta> _metaSerializer;
		private readonly ControllersRouter _controllersRouter;
		private readonly RequestorCollection _requestors;

		public RpcClient(
			IPAddress iPAddress,
			int port,
			IEnumerable<IControllerHandler> controllerHandlers,
			RequestorCollection requestors
		) : base(iPAddress, port)
		{
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_controllersRouter = new ControllersRouter(controllerHandlers);
			_requestors = requestors;
		}

		protected override void NotifyStarted() => _requestors.InitializeRequestors(TcpCommunication);

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
						{
							_controllersRouter.RouteToController(tcpCommunication, meta, buffer, offset);
							break;
						}
					case CommunicationMeta.PackageType.Response:
						{
							_requestors.HandleResponse(meta, buffer, offset);
							break;
						}
					default: throw new Exception($"Invalid value {meta.Type} for {nameof(meta.Type)}.");
				}
			});
		}
	}
}
