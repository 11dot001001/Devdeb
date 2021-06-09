using System;

namespace Devdeb.DependencyInjection.Extensions
{
	public static class ServiceProviderExtensions
	{
		static public object GetRequiredService(this IServiceProvider serviceProvider, Type serviceType)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var service = serviceProvider.GetService(serviceType);

			if (service == null)
				throw new Exception($"Required service {serviceType.FullName} doesn't configured in service collection.");

			return service;
		}
		static public T GetRequiredService<T>(this IServiceProvider serviceProvider) => (T)GetRequiredService(serviceProvider, typeof(T));
		static public T GetService<T>(this IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			return (T)serviceProvider.GetService(typeof(T));
		}
	}
}
