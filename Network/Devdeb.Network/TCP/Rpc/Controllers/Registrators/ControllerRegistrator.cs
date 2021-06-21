using System;
using System.Collections.Generic;

namespace Devdeb.Network.TCP.Rpc.Controllers.Registrators
{
	internal class ControllerRegistrator : IControllerRegistrator
	{
		internal class ControllerConfig
		{
			public Type InterfaceType { get; set; }
			public Type ImplementationType { get; set; }
		}

		private readonly List<ControllerConfig> _configurations;

		public ControllerRegistrator() => _configurations = new List<ControllerConfig>();

		internal List<ControllerConfig> Configurations => _configurations;

		public void AddController<TInterface, TImplementation>() where TImplementation : TInterface
		{
			_configurations.Add(new ControllerConfig
			{
				InterfaceType = typeof(TInterface),
				ImplementationType = typeof(TImplementation),
			});
		}
	}
}
