using Devdeb.DependencyInjection;
using Devdeb.Network.TCP.Rpc.Pipelines;
using Devdeb.Network.TCP.Rpc.Requestor;
using System;
using System.Collections.Generic;

namespace Devdeb.Network.TCP.Rpc
{
	public interface IStartup
	{
		Type RequestorType { get; }
		Func<RequestorCollection> CreateRequestor { get; }
		void AddControllers(Dictionary<Type, Type> controllerSurjection);
		void ConfigureHostedServices(List<Type> hostedServices);
		void ConfigureServices(IServiceCollection serviceCollection);
		void ConfigurePipeline(IPipelineBuilder pipelineBuilder);
	}
}
