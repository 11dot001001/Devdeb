namespace Devdeb.Network.TCP.Rpc.Controllers.Registrators
{
	public interface IControllerRegistrator
	{
		void AddController<TInterface, TImplementation>() where TImplementation : TInterface;
	}
}
