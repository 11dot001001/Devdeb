using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.TCP.Rpc.Communication;
using Devdeb.Network.TCP.Rpc.Controllers;
using Devdeb.Network.TCP.Rpc.Requestor;
using Devdeb.Network.TCP.Rpc.Requestor.Context;
using System;
using System.Threading.Tasks;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.TCP.Rpc.Pipelines
{
	public static class PipelineBuilderExtensions
	{
		static public void UseRequestLogger(this IPipelineBuilder pipelineBuilder)
		{
			pipelineBuilder.Use((IServiceProvider serviceProvider, NextMiddlewareDelegate nextMiddleware) =>
			{
				var requestorContext = serviceProvider.GetRequiredService<IRequestorContext>();
				Guid requestId = Guid.NewGuid();
				Console.WriteLine(
					$"REQUEST LOGGER {requestId}\n" +
					$"From: {requestorContext.TcpCommunication.Socket.RemoteEndPoint}.\n" +
					$"Data length: {requestorContext.Data?.Length}.\n"
				);

				if (nextMiddleware != null)
				{
					return nextMiddleware().ContinueWith(previousTask =>
					{
						Console.WriteLine(
							$"REQUEST LOGGER {requestId}\n" +
							$"Request task status {previousTask.Status}.\n"
						);
					});
				};

				return Task.CompletedTask;
			});
		}

		static public void UseControllers(this IPipelineBuilder pipelineBuilder)
		{
			pipelineBuilder.Use((IServiceProvider serviceProvider, NextMiddlewareDelegate nextMiddleware) =>
			{
				IRequestorContext context = serviceProvider.GetRequiredService<IRequestorContext>();
				RequestorCollection requestorCollection = serviceProvider.GetRequiredService<RequestorCollection>();
				ControllersRouter controllersRouter = serviceProvider.GetRequiredService<ControllersRouter>();

				CommunicationMeta meta = context.CommunicationMeta;

				switch (meta.Type)
				{
					case CommunicationMeta.PackageType.Request:
						controllersRouter.RouteToController(serviceProvider, context.TcpCommunication, meta, context.Data, 0);
						break;
					case CommunicationMeta.PackageType.Response:
						{
							requestorCollection.HandleResponse(meta, context.Data, 0);
							break;
						}
					default: throw new Exception($"Invalid value {meta.Type} for {nameof(meta.Type)}.");
				}

				return nextMiddleware == null ? Task.CompletedTask : nextMiddleware.Invoke();
			});
		}
	}
}
