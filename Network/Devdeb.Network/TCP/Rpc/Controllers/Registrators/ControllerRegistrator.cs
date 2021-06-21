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

		public ControllerRegistrator() => Configurations = new List<ControllerConfig>();

		internal List<ControllerConfig> Configurations { get; }

		public void AddController<TInterface, TImplementation>() where TImplementation : TInterface
		{
			Configurations.Add(new ControllerConfig
			{
				InterfaceType = typeof(TInterface),
				ImplementationType = typeof(TImplementation),
			});
		}
	}
}
