using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Communication;
using Devdeb.Network.TCP.Expecting;
using Devdeb.Network.TCP.Rpc.Communication;
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
using static Devdeb.Network.TCP.Rpc.HostedServices.Registrators.HostedServiceRegistrator;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc
{
	public sealed class RpcClient : BaseExpectingTcpClient
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly List<HostedServiceConfig> _hostedServices;
		private readonly PipelineEntryPointDelegate _runPipeline;

		private TcpCommunication _tcpCommunication;

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
			RequestorCollection requestor = (RequestorCollection)Activator.CreateInstance(requestorType);
			serviceCollection.AddSingleton(_ => requestor);
			serviceCollection.AddSingleton(requestorType, _ => requestor);

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
			base.Connected(_tcpCommunication = tcpCommunication);
			_serviceProvider.GetRequiredService<RequestorCollection>().InitializeRequestors(_tcpCommunication);
		}
		protected override void Disconnected()
		{
			// stop all client handlers and other...
		}

		protected override void ProcessCommunication(int receivedCount)
		{
			CommunicationMeta meta = _tcpCommunication.Receive(CommunicationMetaSerializer.Default);
			receivedCount -= CommunicationMetaSerializer.Default.Size;

			byte[] buffer = null;
			if (receivedCount != 0)
			{
				buffer = new byte[receivedCount];
				_tcpCommunication.Receive(buffer, 0, buffer.Length);
			}

			Task.Factory.StartNew(async () =>
			{
				IServiceProvider scopedServiceProvider = _serviceProvider.CreateScope();
				RequestorContext context = (RequestorContext)scopedServiceProvider.GetRequiredService<IRequestorContext>();
				context.SetTcpCommunication(_tcpCommunication);
				context.SetCommunicationMeta(meta);
				context.SetData(buffer);

				await _runPipeline(scopedServiceProvider);
			});
		}
	}
}
