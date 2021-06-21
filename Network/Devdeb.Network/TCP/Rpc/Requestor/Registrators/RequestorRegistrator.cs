using System;

namespace Devdeb.Network.TCP.Rpc.Requestor.Registrators
{
	internal class RequestorRegistrator : IRequestorRegistrator
	{
		internal class RequestorConfig
		{
			public Type ImplementationType { get; set; }
		}

		internal RequestorConfig Configuration { get; private set; }

		public void UseRequestor<TImplementation>() where TImplementation : RequestorCollection, new()
		{
			Configuration = new RequestorConfig
			{ 
				ImplementationType = typeof(TImplementation)
			};
		}
	}
}
