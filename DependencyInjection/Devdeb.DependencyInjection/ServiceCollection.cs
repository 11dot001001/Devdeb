using System;
using System.Collections.Generic;

namespace Devdeb.DependencyInjection
{
	public class ServiceCollection : IServiceCollection
	{
		internal class ServiceСonfiguration
		{
			public Type ServiceType { get; set; }
			public Type ImplementationType { get; set; }
			public LifeTimeType LifeTimeType { get; set; }
			public Func<IServiceProvider, object> Initialize { get; set; }
		}

		private readonly Dictionary<Type, ServiceСonfiguration> _configurations;

		public ServiceCollection()
		{
			_configurations = new Dictionary<Type, ServiceСonfiguration>();
		}

		public void AddScoped(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize)
		{
			VerifyArguments(serviceType, implementationType, initialize);
			_configurations.Add(serviceType, new ServiceСonfiguration
			{
				ServiceType = serviceType,
				ImplementationType = implementationType,
				LifeTimeType = LifeTimeType.Scoped,
				Initialize = initialize
			});
		}
		public void AddSingleton(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize)
		{
			VerifyArguments(serviceType, implementationType, initialize);
			_configurations.Add(serviceType, new ServiceСonfiguration
			{
				ServiceType = serviceType,
				ImplementationType = implementationType,
				LifeTimeType = LifeTimeType.Singleton,
				Initialize = initialize
			});
		}
		public void AddTransient(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize)
		{
			VerifyArguments(serviceType, implementationType, initialize);
			_configurations.Add(serviceType, new ServiceСonfiguration
			{
				ServiceType = serviceType,
				ImplementationType = implementationType,
				LifeTimeType = LifeTimeType.Transient,
				Initialize = initialize
			});
		}

		private void VerifyArguments(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			if (implementationType == null)
				throw new ArgumentNullException(nameof(implementationType));
			if (initialize == null)
				throw new ArgumentNullException(nameof(initialize));

			if (!serviceType.IsAssignableFrom(implementationType))
				throw new Exception($"Specified implementation type {implementationType.FullName} can't be assign to service type {serviceType.FullName}.");
		}

		public IServiceProvider BuildServiceProvider() => new ServiceProvider(_configurations);
	}
}
