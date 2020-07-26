using System.Net.Sockets;

namespace Devdeb.Network.Connection
{
	public interface IConnectionProvider<Package> where Package : IConnectionPackage
	{
		Socket Connection { get; }
		int ReceivedPackagesCount { get; }
		int SendingPackagesCount { get; }
		void AddPackageToSend(Package package);
		Package GetPackage();
		void SendBytes();
		void ReceiveBytes();
	}
}