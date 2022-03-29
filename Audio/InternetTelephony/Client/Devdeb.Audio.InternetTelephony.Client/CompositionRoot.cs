using Devdeb.Audio.InternetTelephony.Client.Audio;
using Devdeb.Audio.InternetTelephony.Client.Controllers;
using Devdeb.Audio.InternetTelephony.Client.Services;
using Devdeb.DependencyInjection;
using Devdeb.DependencyInjection.Extensions;

namespace Devdeb.Audio.InternetTelephony.Client
{
	internal static class CompositionRoot
	{
		internal static void AddDomain(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<AudioPlayer>();
			serviceCollection.AddSingleton<CallAcceptanceRequestor>();
		}
	}
}
