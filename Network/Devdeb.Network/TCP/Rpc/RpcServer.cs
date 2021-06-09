using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Handler;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Serialization;
using Devdeb.Serialization.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcServer : BaseExpectingTcpServer
	{
		private readonly ISerializer<CommunicationMeta> _metaSerializer;
		private readonly Dictionary<TcpCommunication, RequestorCollection> _connections;
		private readonly ControllersRouter _controllersRouter;
		private readonly Func<RequestorCollection> _createRequestors;
		private readonly IServiceProvider _serviceProvider;

		public RpcServer(
			IPAddress iPAddress,
			int port,
			int backlog,
			Dictionary<Type, Type> controllers,
			Func<RequestorCollection> createRequestors,
			Action<IServiceCollection> addServices = null
		) : base(iPAddress, port, backlog)
		{
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_connections = new Dictionary<TcpCommunication, RequestorCollection>();
			_createRequestors = createRequestors;

			ServiceCollection serviceCollection = new ServiceCollection();
			_controllersRouter = new ControllersRouter(controllers.Select(controllerSurjection =>
			{
				serviceCollection.AddSingleton(controllerSurjection.Key, controllerSurjection.Value);
				return (IControllerHandler)Activator.CreateInstance(typeof(ControllerHandler<>).MakeGenericType(controllerSurjection.Key));
			}));

			addServices?.Invoke(serviceCollection);
			_serviceProvider = serviceCollection.BuildServiceProvider();
		}

		protected override void ProcessAccept(TcpCommunication tcpCommunication)
		{
			base.ProcessAccept(tcpCommunication);

			RequestorCollection requestor = _createRequestors();
			requestor.InitializeRequestors(tcpCommunication);
			_connections.Add(tcpCommunication, requestor);
		}

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int count)
		{
			byte[] buffer = new byte[count];
			tcpCommunication.Receive(buffer, 0, count);

			Task.Factory.StartNew(() =>
			{
				int offset = 0;
				CommunicationMeta meta = _metaSerializer.Deserialize(buffer, ref offset);

				IServiceProvider scopedServiceProvider = _serviceProvider.CreateScope();

				switch (meta.Type)
				{
					case CommunicationMeta.PackageType.Request:
						_controllersRouter.RouteToController(scopedServiceProvider, tcpCommunication, meta, buffer, offset);
						break;
					case CommunicationMeta.PackageType.Response:
						{
							RequestorCollection requestors = _connections[tcpCommunication];
							requestors.HandleResponse(meta, buffer, offset);
							break;
						}
					default: throw new Exception($"Invalid value {meta.Type} for {nameof(meta.Type)}.");
				}
				TestClientRequest(_connections[tcpCommunication]);
			});
		}

		public Action<RequestorCollection> TestClientRequest { get; set; }
	}
}
