using System.Net.Sockets;

namespace Devdeb.Network
{
	public class TCPConnection
	{
		private readonly Socket _connection;
		private readonly byte[] _sendBuffer;
		private readonly byte[] _receiveBuffer;
	}
}