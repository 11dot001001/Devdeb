namespace Devdeb.Network.TCP.Rpc.Requestor.Registrators
{
	public interface IRequestorRegistrator
	{
		void UseRequestor<TImplementation>() where TImplementation : RequestorCollection, new();
	}
}
