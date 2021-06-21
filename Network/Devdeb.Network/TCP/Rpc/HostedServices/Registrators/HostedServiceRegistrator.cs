using System;
using System.Collections.Generic;

namespace Devdeb.Network.TCP.Rpc.HostedServices.Registrators
{
	internal class HostedServiceRegistrator : IHostedServiceRegistrator
	{
		internal class HostedServiceConfig
		{
			public Type ImplementationType { get; set; }
		}

		public HostedServiceRegistrator() => Configurations = new List<HostedServiceConfig>();
		
		internal List<HostedServiceConfig> Configurations { get; }

		public void AddHostedService<TImplementation>() where TImplementation : IHostedService
		{
			Configurations.Add(new HostedServiceConfig
			{
				ImplementationType = typeof(TImplementation),
			});
		}
	}
}
