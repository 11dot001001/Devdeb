using System;
using static Devdeb.DependencyInjection.ServiceCollection;
using System.Collections.Generic;

namespace Devdeb.DependencyInjection
{
	internal class ServiceProvider : IServiceProvider
	{
		private readonly Dictionary<Type, ServiceСonfiguration> _configurations;
		private readonly Dictionary<Type, object> _scopedServices;
		private readonly Dictionary<Type, object> _singletonServices;
		private readonly IServiceProvider _rootServiceProvider;

		public ServiceProvider(Dictionary<Type, ServiceСonfiguration> configurations) : this(configurations, null) 
		{
			Type servicProviderInterface = typeof(IServiceProvider);

			if (configurations.TryGetValue(servicProviderInterface, out _))
				throw new Exception($"Unable to register type {nameof(IServiceProvider)}");

			_configurations.Add(servicProviderInterface, new ServiceСonfiguration
			{
				ServiceType = servicProviderInterface,
				ImplementationType = typeof(ServiceProvider),
				LifeTimeType = LifeTimeType.Scoped,
				Initialize = null
			});
		}
		internal ServiceProvider(Dictionary<Type, ServiceСonfiguration> configurations, IServiceProvider rootServiceProvider)
		{
			_configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
			_rootServiceProvider = rootServiceProvider;
			_scopedServices = new Dictionary<Type, object>();
			_singletonServices = new Dictionary<Type, object>();

			_scopedServices.Add(typeof(IServiceProvider), this);
		}

		public object GetService(Type serviceType)
		{
			ServiceСonfiguration serviceConfiguration = _configurations[serviceType];

			switch (serviceConfiguration.LifeTimeType)
			{
				case LifeTimeType.Singleton:
					{
						if (_rootServiceProvider != null)
							return _rootServiceProvider.GetService(serviceType);

						if (!_singletonServices.TryGetValue(serviceType, out object instance))
						{
							lock (_singletonServices)
							{
								if (!_singletonServices.TryGetValue(serviceType, out instance))
									_singletonServices.Add(serviceType, instance = serviceConfiguration.Initialize(this));
							}
						}
						return instance;
					}
				case LifeTimeType.Scoped:
					{
						if (!_scopedServices.TryGetValue(serviceType, out object instance))
						{
							lock (_scopedServices)
							{
								if (!_scopedServices.TryGetValue(serviceType, out instance))
									_scopedServices.Add(serviceType, instance = serviceConfiguration.Initialize(this));
							}
						}
						return instance;
					}
				case LifeTimeType.Transient:
					return serviceConfiguration.Initialize(this);
				default:
					throw new Exception($"Invalid {nameof(ServiceСonfiguration.LifeTimeType)}: {serviceConfiguration.LifeTimeType} for type {serviceType.FullName}.");
			}
		}
		public IServiceProvider CreateScope() => new ServiceProvider(_configurations, this);
	}
}
