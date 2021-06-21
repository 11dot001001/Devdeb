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

		private readonly List<HostedServiceConfig> _configurations;

		public HostedServiceRegistrator() => _configurations = new List<HostedServiceConfig>();
		
		internal List<HostedServiceConfig> Configurations => _configurations;

		public void AddHostedService<TImplementation>() where TImplementation : IHostedService
		{
			_configurations.Add(new HostedServiceConfig
			{
				ImplementationType = typeof(TImplementation),
			});
		}
	}
}
