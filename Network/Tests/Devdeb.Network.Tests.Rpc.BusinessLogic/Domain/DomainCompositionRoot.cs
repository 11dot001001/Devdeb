using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Devdeb.Network.Tests.Rpc.BusinessLogic.Domain.Abstractions;

namespace Devdeb.Network.Tests.Rpc.BusinessLogic.Domain
{
	static public class DomainCompositionRoot
	{
		static public IServiceCollection AddDomain(this IServiceCollection services)
		{
			services.AddScoped<IDateTimeService, DateTimeService>();

			return services;
		}
	}
}
