namespace Devdeb.Network.Connection
{
	public interface IConnectionPackage
	{
		ConnectionPackageType Type { get; }
		int DataLenght { get; }
		byte[] Data { get; }
	}
}