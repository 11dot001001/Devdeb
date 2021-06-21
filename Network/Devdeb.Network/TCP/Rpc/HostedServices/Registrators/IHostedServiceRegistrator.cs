namespace Devdeb.Network.TCP.Rpc.HostedServices.Registrators
{
	public interface IHostedServiceRegistrator
	{
		void AddHostedService<TImplementation>() where TImplementation : IHostedService;
	}
}
