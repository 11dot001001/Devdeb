using System.Threading.Tasks;

namespace Devdeb.Network.TCP.Rpc
{
	public interface IHostedService
	{
		Task StartAsync();
		Task StopAsync();
	}
}
