using System;

namespace Devdeb.DependencyInjection
{
	public interface IServiceCollection
	{
		void AddScoped(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize);
		void AddSingleton(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize);
		void AddTransient(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize);
		IServiceProvider BuildServiceProvider();
	}
}
