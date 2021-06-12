using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;
using Server.Domain.Services;
using Server.Domain.Services.Abstractions;

namespace Server.Domain
{
	static public class DomainCompositionRoot
	{
		static public IServiceCollection AddDomain(this IServiceCollection services)
		{
			services.AddScoped<IStudentService, StudentService>();
			services.AddScoped<ITeacherService, TeacherService>();

			return services;
		}
	}
}
