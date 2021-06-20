using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Handler;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc.Requestor.Context;
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
		private readonly Dictionary<TcpCommunication, RequestorCollection> _connectionRequestors;
		private readonly Func<RequestorCollection> _createRequestors;
		private readonly IServiceProvider _serviceProvider;
		private readonly List<Type> _hostedServices;
		private readonly PipelineEntryPointDelegate _runPipeline;

		public RpcServer(IPAddress iPAddress, int port, int backlog, IStartup startup) : base(iPAddress, port, backlog)
		{
			_connectionRequestors = new Dictionary<TcpCommunication, RequestorCollection>();
			_createRequestors = startup.CreateRequestor;

			ServiceCollection serviceCollection = new ServiceCollection();

			Dictionary<Type, Type> controllerSurjection = new Dictionary<Type, Type>();
			startup.AddControllers(controllerSurjection);
			ControllersRouter controllersRouter = new ControllersRouter(controllerSurjection.Select(controller =>
			{
				serviceCollection.AddScoped(controller.Key, controller.Value);
				return (IControllerHandler)Activator.CreateInstance(typeof(ControllerHandler<>).MakeGenericType(controller.Key));
			}));
			serviceCollection.AddSingleton<ControllersRouter, ControllersRouter>(_ => controllersRouter);
			serviceCollection.AddScoped<RequestorCollection, RequestorCollection>(serviceProvider =>
			{
				IRequestorContext requestorContext = serviceProvider.GetRequiredService<IRequestorContext>();
				return _connectionRequestors[requestorContext.TcpCommunication];
			});

			serviceCollection.AddScoped<IRequestorContext, RequestorContext>();
			serviceCollection.AddScoped(startup.RequestorType, startup.RequestorType, serviceProvider =>
			{
				IRequestorContext requestorContext = serviceProvider.GetRequiredService<IRequestorContext>();
				return _connectionRequestors[requestorContext.TcpCommunication];
			});

			startup.ConfigureServices(serviceCollection);

			startup.ConfigureHostedServices(_hostedServices = new List<Type>());
			_hostedServices.ForEach(serviceType => serviceCollection.AddSingleton(serviceType));

			PipelineBuilder pipelineBuilder = new PipelineBuilder();
			startup.ConfigurePipeline(pipelineBuilder);
			_runPipeline = pipelineBuilder.Build();

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

		protected override void ProcessCommunication(TcpCommunication tcpCommunication, int receivedCount)
		{
			CommunicationMeta meta = tcpCommunication.Receive(CommunicationMetaSerializer.Default);
			receivedCount -= CommunicationMetaSerializer.Default.Size;

			byte[] buffer = null;
			if (receivedCount != 0)
			{
				buffer = new byte[receivedCount];
				tcpCommunication.Receive(buffer, 0, buffer.Length);
			}

			Task.Factory.StartNew(async () =>
			{
				IServiceProvider scopedServiceProvider = _serviceProvider.CreateScope();

				RequestorContext context = (RequestorContext)scopedServiceProvider.GetRequiredService<IRequestorContext>();
				context.SetTcpCommunication(tcpCommunication);
				context.SetCommunicationMeta(meta);
				context.SetData(buffer);

				await _runPipeline(scopedServiceProvider);
			});
		}
	}
}
