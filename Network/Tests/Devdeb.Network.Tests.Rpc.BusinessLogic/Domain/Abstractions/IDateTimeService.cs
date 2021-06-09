using System;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions
{
	public interface IDateTimeService
	{
		DateTime CurrentDateTime { get; }
	}
}
