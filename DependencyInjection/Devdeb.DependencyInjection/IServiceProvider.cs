using System;

namespace Devdeb.DependencyInjection
{
	public interface IServiceProvider
	{
		object GetService(Type serviceType);
		IServiceProvider CreateScope();
	}
}
