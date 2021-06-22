using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Connections;
using Devdeb.Network.TCP.Rpc.Controllers;
using Devdeb.Network.TCP.Rpc.Controllers.Registrators;
using Devdeb.Network.TCP.Rpc.HostedServices;
using Devdeb.Network.TCP.Rpc.HostedServices.Registrators;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc.Requestor.Context;
using Devdeb.Network.TCP.Rpc.Requestor.Registrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Devdeb.Network.TCP.Rpc.Connections.Connection;
using static Devdeb.Network.TCP.Rpc.HostedServices.Registrators.HostedServiceRegistrator;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcClient : BaseExpectingTcpClient
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly List<HostedServiceConfig> _hostedServices;
		private readonly PipelineEntryPointDelegate _runPipeline;
		private readonly RequestorCollection _requestors;
		private Connection _connection;

		public RpcClient(IPAddress iPAddress, int port, IStartup startup) : base(iPAddress, port)
		{
			RequestorRegistrator requestorRegistrator = new RequestorRegistrator();
			ControllerRegistrator controllerRegistrator = new ControllerRegistrator();
			HostedServiceRegistrator hostedServiceRegistrator = new HostedServiceRegistrator();
			ServiceCollection serviceCollection = new ServiceCollection();
			PipelineBuilder pipelineBuilder = new PipelineBuilder();

			startup.ConfigureRequestor(requestorRegistrator);
			startup.ConfigureControllers(controllerRegistrator);
			startup.ConfigureHostedServices(hostedServiceRegistrator);
			startup.ConfigureServices(serviceCollection);
			startup.ConfigurePipeline(pipelineBuilder);

			Type requestorType = requestorRegistrator.Configuration.ImplementationType;
			_requestors = (RequestorCollection)Activator.CreateInstance(requestorType);
			serviceCollection.AddSingleton(_ => _requestors);
			serviceCollection.AddSingleton(requestorType, _ => _requestors);

			ControllersRouter controllersRouter = new ControllersRouter(controllerRegistrator.Configurations.Select(controller =>
			{
				serviceCollection.AddScoped(controller.InterfaceType, controller.ImplementationType);
				return (IControllerHandler)Activator.CreateInstance(typeof(ControllerHandler<>).MakeGenericType(controller.InterfaceType));
			}));
			serviceCollection.AddSingleton(_ => controllersRouter);

			serviceCollection.AddScoped<IRequestorContext, RequestorContext>();

			_hostedServices = hostedServiceRegistrator.Configurations;
			_hostedServices.ForEach(hostedServiceConfig =>
			{
				serviceCollection.AddSingleton(hostedServiceConfig.ImplementationType);
			});

			_runPipeline = pipelineBuilder.Build();

			_serviceProvider = serviceCollection.BuildServiceProvider();
		}

		public override void Start()
		{
			base.Start();
			_hostedServices.ForEach(serviceType =>
			{
				IServiceProvider scopedServiceProvider = _serviceProvider.CreateScope();
				IHostedService service = (IHostedService)scopedServiceProvider.GetService(serviceType.ImplementationType);
				_ = Task.Run(service.StartAsync);
			});
		}

		protected override void Connected(TcpCommunication tcpCommunication)
		{
			base.Connected(tcpCommunication);
			_requestors.InitializeRequestors(tcpCommunication);
			_connection = new Connection(tcpCommunication, _requestors);
		}
		protected override void Disconnected() => _connection.Close();

		protected override void ProcessCommunication(int receivedCount)
		{
			CommunicationMeta meta = _connection.TcpCommunication.Receive(CommunicationMetaSerializer.Default);
			receivedCount -= CommunicationMetaSerializer.Default.Size;

			byte[] buffer = null;
			if (receivedCount != 0)
			{
				buffer = new byte[receivedCount];
				_connection.TcpCommunication.Receive(buffer, 0, buffer.Length);
			}

			IServiceProvider scopedServiceProvider = _serviceProvider.CreateScope();
			ProcessingContext processingContext = new ProcessingContext();

			Task<Task<bool>> processingTask = Task.Factory.StartNew(() =>
			{
				return StartProcessing(_connection.TcpCommunication, scopedServiceProvider, meta, buffer)
					   .ContinueWith(x => _connection.ProcessingContexts.Remove(processingContext));
			});

			processingContext.ServiceProvider = scopedServiceProvider;
			processingContext.ProcessingTask = processingTask;
			_connection.ProcessingContexts.Add(processingContext);
		}

		private async Task StartProcessing(
			TcpCommunication tcpCommunication,
			IServiceProvider serviceProvider,
			CommunicationMeta meta,
			byte[] buffer
		)
		{
			RequestorContext context = (RequestorContext)serviceProvider.GetRequiredService<IRequestorContext>();
			context.SetTcpCommunication(tcpCommunication);
			context.SetCommunicationMeta(meta);
			context.SetData(buffer);

			await _runPipeline(serviceProvider);
		}
	}
}
