using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions;
using System;
using IServiceProvider = Devdeb.DependencyInjection.IServiceProvider;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain
{
	internal class DateTimeService : IDateTimeService
	{
		private readonly IServiceProvider _serviceProvider;

		public DateTimeService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public DateTime CurrentDateTime { get => DateTime.Now; }
	}
}
