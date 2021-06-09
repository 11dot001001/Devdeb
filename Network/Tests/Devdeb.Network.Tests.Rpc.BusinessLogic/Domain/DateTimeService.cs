using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions;
using System;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain
{
	internal class DateTimeService : IDateTimeService
	{
		public DateTime CurrentDateTime { get => DateTime.Now; }
	}
}
