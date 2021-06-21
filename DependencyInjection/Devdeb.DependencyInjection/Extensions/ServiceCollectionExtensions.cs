using System;

namespace Devdeb.DependencyInjection.Extensions
{
	public static class ServiceCollectionExtensions
	{
		static public void AddSingleton(this IServiceCollection collection, Type serviceType, Func<IServiceProvider, object> initialize = null)
		{
			collection.AddSingleton(serviceType, serviceType, initialize);
		}
		static public void AddSingleton<TService>(this IServiceCollection collection, Func<IServiceProvider, TService> initialize = null)
			where TService : class
		{
			collection.AddSingleton(typeof(TService), typeof(TService), initialize);
		}
		static public void AddSingleton<TService, TImplementation>(this IServiceCollection collection, Func<IServiceProvider, TImplementation> initialize = null)
			where TImplementation : class, TService
		{
			collection.AddSingleton(typeof(TService), typeof(TImplementation), initialize);
		}

		static public void AddScoped(this IServiceCollection collection, Type serviceType, Func<IServiceProvider, object> initialize = null)
		{
			collection.AddScoped(serviceType, serviceType, initialize);
		}
		static public void AddScoped<TService>(this IServiceCollection collection, Func<IServiceProvider, TService> initialize = null)
			where TService : class
		{
			collection.AddScoped(typeof(TService), typeof(TService), initialize);
		}
		static public void AddScoped<TService, TImplementation>(this IServiceCollection collection, Func<IServiceProvider, TImplementation> initialize = null)
			where TImplementation : class, TService
		{
			collection.AddScoped(typeof(TService), typeof(TImplementation), initialize);
		}

		static public void AddTransient(this IServiceCollection collection, Type serviceType, Func<IServiceProvider, object> initialize = null)
		{
			collection.AddTransient(serviceType, serviceType, initialize);
		}
		static public void AddTransient<TService>(this IServiceCollection collection, Func<IServiceProvider, TService> initialize = null)
			where TService : class
		{
			collection.AddTransient(typeof(TService), typeof(TService), initialize);
		}
		static public void AddTransient<TService, TImplementation>(this IServiceCollection collection, Func<IServiceProvider, TImplementation> initialize = null)
			where TImplementation : class, TService
		{
			collection.AddTransient(typeof(TService), typeof(TImplementation), initialize);
		}
	}
}
