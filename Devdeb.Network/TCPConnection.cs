using System;
using System.Net.Sockets;

namespace Devdeb.Network
{
	internal class TCPConnection
	{
		private readonly Socket _connection;
		private readonly ConnectionBuffer _sendBuffer;
		private readonly ConnectionBuffer _receiveBuffer;

		internal void SendBytes()
		{
			int maxRightSize = _sendBuffer.Capacity - _sendBuffer.ReadIndex;
			if (maxRightSize >= count)
			{
				Array.Copy(_buffer, _readIndex, bytes, 0, count);
				_readIndex += count;
			}
			else
			{
				Array.Copy(_buffer, _readIndex, bytes, 0, maxRightSize);
				int residualAmount = count - maxRightSize;
				Array.Copy(_buffer, 0, bytes, maxRightSize, residualAmount);
				_readIndex = residualAmount;
			}
			_count -= count;


			int sentBytesCount = _connection.Send(_sendBuffer.Buffer, _sendBuffer.ReadIndex, _sendBuffer.Count, SocketFlags.None, out SocketError socketError);
			if (socketError != SocketError.Success)
				throw new System.Exception($"{nameof(SocketError)} is {socketError}");
		}

		public void SendPackage(byte[] bytes)
		{
		}
		public bool TryReceivePackage(out byte[] bytes)
		{

		}
	}
}