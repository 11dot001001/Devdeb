using Client.Domain.Services;
using Client.Domain.Services.Abstractions;
using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;

namespace Client.Domain
{
	static public class DomainCompositionRoot
	{
		static public IServiceCollection AddDomain(this IServiceCollection services)
		{
			services.AddScoped<IStudentService, StudentService>();

			return services;
		}
	}
}
