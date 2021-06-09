using Devdeb.DependencyInjection.Extensions;
using Devdeb.DependencyInjection.Tests.Domain;
using Devdeb.DependencyInjection.Tests.Domain.Abstractions;

namespace Devdeb.DependencyInjection.Tests
{
	class Program
	{
		static void Main(string[] args)
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton<IStudentService, StudentService>();
			serviceCollection.AddTransient<IUniversityService, UniversityService>();

			IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
			IStudentService studentService1 = serviceProvider.GetService<IStudentService>();
			IStudentService studentService2 = serviceProvider.GetService<IStudentService>();
			IStudentService studentService3 = serviceProvider.GetService<IStudentService>();

			IServiceProvider serviceProvider2 = serviceProvider.CreateScope();
			IStudentService studentService12 = serviceProvider2.GetService<IStudentService>();
			IStudentService studentService22 = serviceProvider2.GetService<IStudentService>();
			IStudentService studentService32 = serviceProvider2.GetService<IStudentService>();
		}
	}
}
