using System;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace Devdeb.DependencyInjection.Extensions
{
	public static class ServiceCollectionExtensions
	{
		static public void AddSingleton(this IServiceCollection collection, Type serviceType)
		{
			collection.AddSingleton(serviceType, serviceType, GetDefaultInitialization(serviceType));
		}
		static public void AddSingleton<TService>(this IServiceCollection collection) where TService : class
		{
			AddSingleton(collection, typeof(TService));
		}
		static public void AddSingleton<TService, TImplementation>(this IServiceCollection collection) where TImplementation : class
		{
			Type implementationType = typeof(TImplementation);

			collection.AddSingleton(typeof(TService), implementationType, GetDefaultInitialization(implementationType));
		}
		static public void AddSingleton<TService, TImplementation>(this IServiceCollection collection, Func<IServiceProvider, TImplementation> initialize)
			where TImplementation : class
		{
			if (initialize == null)
				throw new ArgumentNullException(nameof(initialize));

			collection.AddSingleton(typeof(TService), typeof(TImplementation), initialize);
		}

		static public void AddScoped(this IServiceCollection collection, Type serviceType)
		{
			collection.AddScoped(serviceType, serviceType, GetDefaultInitialization(serviceType));
		}
		static public void AddScoped<TService>(this IServiceCollection collection) where TService : class
		{
			AddScoped(collection, typeof(TService));
		}
		static public void AddScoped<TService, TImplementation>(this IServiceCollection collection) where TImplementation : class
		{
			Type implementationType = typeof(TImplementation);

			collection.AddScoped(typeof(TService), implementationType, GetDefaultInitialization(implementationType));
		}
		static public void AddScoped<TService, TImplementation>(this IServiceCollection collection, Func<IServiceProvider, TImplementation> initialize)
			where TImplementation : class
		{
			if (initialize == null)
				throw new ArgumentNullException(nameof(initialize));

			collection.AddScoped(typeof(TService), typeof(TImplementation), initialize);
		}

		static public void AddTransient(this IServiceCollection collection, Type serviceType)
		{
			collection.AddTransient(serviceType, serviceType, GetDefaultInitialization(serviceType));
		}
		static public void AddTransient<TService>(this IServiceCollection collection) where TService : class
		{
			AddTransient(collection, typeof(TService));
		}
		static public void AddTransient<TService, TImplementation>(this IServiceCollection collection) where TImplementation : class
		{
			Type implementationType = typeof(TImplementation);

			collection.AddTransient(typeof(TService), implementationType, GetDefaultInitialization(implementationType));
		}
		static public void AddTransient<TService, TImplementation>(this IServiceCollection collection, Func<IServiceProvider, TImplementation> initialize)
			where TImplementation : class
		{
			if (initialize == null)
				throw new ArgumentNullException(nameof(initialize));

			collection.AddTransient(typeof(TService), typeof(TImplementation), initialize);
		}

		static private Func<IServiceProvider, object> GetDefaultInitialization(Type implementationType)
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
