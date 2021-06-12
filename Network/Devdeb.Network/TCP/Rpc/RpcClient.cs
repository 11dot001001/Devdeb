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
	public sealed class RpcClient : BaseExpectingTcpClient
	{
		private readonly ISerializer<CommunicationMeta> _metaSerializer;
		private readonly ControllersRouter _controllersRouter;
		private readonly RequestorCollection _serverApi;
		private readonly IServiceProvider _serviceProvider;

		public RpcClient(IPAddress iPAddress, int port, IStartup startup) : base(iPAddress, port)
		{
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_serverApi = startup.CreateRequestor();

			ServiceCollection serviceCollection = new ServiceCollection();

			Dictionary<Type, Type> controllers = new Dictionary<Type, Type>();
			startup.AddControllers(controllers);
			_controllersRouter = new ControllersRouter(controllers.Select(controllerSurjection =>
			{
				serviceCollection.AddScoped(controllerSurjection.Key, controllerSurjection.Value);
				return (IControllerHandler)Activator.CreateInstance(typeof(ControllerHandler<>).MakeGenericType(controllerSurjection.Key));
			}));

			serviceCollection.AddScoped(startup.RequestorType, startup.RequestorType, _ => _serverApi);

			startup.AddServices(serviceCollection);
			_serviceProvider = serviceCollection.BuildServiceProvider();
		}

		protected override void NotifyStarted() => _serverApi.InitializeRequestors(TcpCommunication);

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
						{
							_controllersRouter.RouteToController(scopedServiceProvider, tcpCommunication, meta, buffer, offset);
							break;
						}
					case CommunicationMeta.PackageType.Response:
						{
							_serverApi.HandleResponse(meta, buffer, offset);
							break;
						}
					default: throw new Exception($"Invalid value {meta.Type} for {nameof(meta.Type)}.");
				}
			});
		}
	}
}
