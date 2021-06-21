using System;

namespace Devdeb.DependencyInjection
{
	public interface IServiceCollection
	{
		void AddScoped(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize = null);
		void AddSingleton(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize = null);
		void AddTransient(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize = null);
		IServiceProvider BuildServiceProvider();
	}
}
