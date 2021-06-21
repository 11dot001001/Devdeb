using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc.HostedServices
{
	public interface IHostedService
	{
		Task StartAsync();
		Task StopAsync();
	}
}
