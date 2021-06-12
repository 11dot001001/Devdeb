using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Common.Services.Abstractions;
using Common.Services;

namespace Common
{
	static public class CommonServicesCompositionRoot
	{
		static public IServiceCollection AddCommonServices(this IServiceCollection services)
		{
			services.AddScoped<IDateTimeService, DateTimeService>();

			return services;
		}
	}
}
