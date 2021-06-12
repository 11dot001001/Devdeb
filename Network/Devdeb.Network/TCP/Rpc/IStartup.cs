using Devdeb.DependencyInjection;
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
		void AddHostedServices(List<Type> hostedServices);
		void AddServices(IServiceCollection serviceCollection);
	}
}
