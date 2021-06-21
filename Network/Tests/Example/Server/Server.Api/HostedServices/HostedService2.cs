using System;
using System.Threading.Tasks;
using System.Threading;
using Devdeb.Network.TCP.Rpc.HostedServices;

namespace Server.App.HostedServices
{
	internal class HostedService2 : IHostedService
	{
		public async Task StartAsync()
		{
			Thread.Sleep(10000);
		}

		public Task StopAsync() => throw new NotImplementedException();
	}
}
