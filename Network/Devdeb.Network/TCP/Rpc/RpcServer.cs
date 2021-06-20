using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Handler;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc.Requestor.Context;
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
		private readonly Dictionary<TcpCommunication, RequestorCollection> _connectionRequestors;
		private readonly ControllersRouter _controllersRouter;
		private readonly Func<RequestorCollection> _createRequestors;
		private readonly IServiceProvider _serviceProvider;
		private readonly List<Type> _hostedServices;

		public RpcServer(IPAddress iPAddress, int port, int backlog, IStartup startup) : base(iPAddress, port, backlog)
		{
			_metaSerializer = DefaultSerializer<CommunicationMeta>.Instance;
			_connectionRequestors = new Dictionary<TcpCommunication, RequestorCollection>();
			_createRequestors = startup.CreateRequestor;

			ServiceCollection serviceCollection = new ServiceCollection();

			Dictionary<Type, Type> controllers = new Dictionary<Type, Type>();
			startup.AddControllers(controllers);
			_controllersRouter = new ControllersRouter(controllers.Select(controllerSurjection =>
			{
				serviceCollection.AddScoped(controllerSurjection.Key, controllerSurjection.Value);
				return (IControllerHandler)Activator.CreateInstance(typeof(ControllerHandler<>).MakeGenericType(controllerSurjection.Key));
			}));

			serviceCollection.AddScoped<IRequestorContext, RequestorContext>();
			serviceCollection.AddScoped(
				startup.RequestorType,
				startup.RequestorType,
				x => _connectionRequestors[x.GetRequiredService<IRequestorContext>().TcpCommunication]
			);

			startup.AddServices(serviceCollection);

			startup.AddHostedServices(_hostedServices = new List<Type>());
			_hostedServices.ForEach(serviceType => serviceCollection.AddSingleton(serviceType));

			_serviceProvider = serviceCollection.BuildServiceProvider();
		}

		public override void Start()
		{
			base.Start();
			_hostedServices.ForEach(serviceType =>
			{
				IServiceProvider scopedServiceProvider = _serviceProvider.CreateScope();
				IHostedService service = (IHostedService)scopedServiceProvider.GetService(serviceType);
				_ = Task.Run(service.StartAsync);
			});
		}

		protected override void Disconnected(TcpCommunication tcpCommunication)
		{
			// stop all client handlers and other...
			lock (_connectionRequestors)
				_connectionRequestors.Remove(tcpCommunication);
		}

		protected override void ProcessAccept(TcpCommunication tcpCommunication)
		{
			base.ProcessAccept(tcpCommunication);

			RequestorCollection requestor = _createRequestors();
			requestor.InitializeRequestors(tcpCommunication);
			lock(_connectionRequestors)
				_connectionRequestors.Add(tcpCommunication, requestor);
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

				RequestorContext requestorContext = (RequestorContext)scopedServiceProvider.GetRequiredService<IRequestorContext>();
				requestorContext.SetTcpCommunication(tcpCommunication);

				switch (meta.Type)
				{
					case CommunicationMeta.PackageType.Request:
						_controllersRouter.RouteToController(scopedServiceProvider, tcpCommunication, meta, buffer, offset);
						break;
					case CommunicationMeta.PackageType.Response:
						{
							RequestorCollection requestors = _connectionRequestors[tcpCommunication];
							requestors.HandleResponse(meta, buffer, offset);
							break;
						}
					default: throw new Exception($"Invalid value {meta.Type} for {nameof(meta.Type)}.");
				}
			});
		}
	}
}
