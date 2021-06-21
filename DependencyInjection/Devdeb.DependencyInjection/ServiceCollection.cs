using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

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

		public void AddScoped(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize = null)
		{
			initialize ??= GetDefaultInitialization(implementationType);
			VerifyArguments(serviceType, implementationType, initialize);
			_configurations.Add(serviceType, new ServiceСonfiguration
			{
				ServiceType = serviceType,
				ImplementationType = implementationType,
				LifeTimeType = LifeTimeType.Scoped,
				Initialize = initialize
			});
		}
		public void AddSingleton(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize = null)
		{
			initialize ??= GetDefaultInitialization(implementationType);
			VerifyArguments(serviceType, implementationType, initialize);
			_configurations.Add(serviceType, new ServiceСonfiguration
			{
				ServiceType = serviceType,
				ImplementationType = implementationType,
				LifeTimeType = LifeTimeType.Singleton,
				Initialize = initialize
			});
		}
		public void AddTransient(Type serviceType, Type implementationType, Func<IServiceProvider, object> initialize = null)
		{
			initialize ??= GetDefaultInitialization(implementationType);
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

		private Func<IServiceProvider, object> GetDefaultInitialization(Type implementationType)
		{
			ConstructorInfo[] constructors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

			if (constructors.Length != 1)
				throw new Exception($"For type {implementationType.FullName} with many constructors use explicit func of initialization.");

			ConstructorInfo constructor = constructors[0];

			ParameterInfo[] arguments = constructor.GetParameters();

			ParameterExpression serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
			ParameterExpression[] serviceArguments = new ParameterExpression[arguments.Length];
			BinaryExpression[] assignsServiceArguments = new BinaryExpression[arguments.Length];

			for (int i = 0; i < arguments.Length; i++)
			{
				Type argumentType = arguments[i].ParameterType;

				serviceArguments[i] = Expression.Variable(argumentType);
				Expression<Func<IServiceProvider, object>> getServiceArgument = x => x.GetService(argumentType);

				assignsServiceArguments[i] = Expression.Assign(
					serviceArguments[i],
					Expression.Convert(
						Expression.Invoke(getServiceArgument, serviceProviderParameter),
						argumentType
					)
				);
			}

			BlockExpression blockExpression = Expression.Block(
				serviceArguments,
				assignsServiceArguments.AsEnumerable<Expression>()
									   .Append(Expression.New(constructor, serviceArguments))
			);

			return Expression.Lambda<Func<IServiceProvider, object>>(blockExpression, serviceProviderParameter).Compile();
		}
	}
}
