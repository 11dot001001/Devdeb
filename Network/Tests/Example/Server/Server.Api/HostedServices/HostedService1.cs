using System;
using System.Threading.Tasks;
using System.Threading;
using Common.Services.Abstractions;
using Devdeb.Network.TCP.Rpc.HostedServices;

namespace Server.App.HostedServices
{
	internal class HostedService1 : IHostedService
	{
		private readonly IDateTimeService _dateTimeService;

		public HostedService1(IDateTimeService dateTimeService)
		{
			_dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
		}

		public async Task StartAsync()
		{
			Thread.Sleep(10000);
		}

		public Task StopAsync() => throw new NotImplementedException();
	}
}
