using Devdeb.Audio.InternetTelephony.Server.Cache;
using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;

namespace Devdeb.Audio.InternetTelephony.Server
{
	internal static class CompositionRoot
	{
		internal static void AddDomain(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<UserCache>();
			serviceCollection.AddSingleton<CallContextCache>();
		}
	}
}
