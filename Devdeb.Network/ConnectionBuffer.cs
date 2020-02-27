using System;

namespace Devdeb.Network
{
	internal class ConnectionBuffer
	{
		private const int DefaultBufferCapacity = 1024;

		private byte[] _buffer;
		private int _readIndex;
		private int _writeIndex;
		private int _count;

		public ConnectionBuffer() : this(DefaultBufferCapacity) { }
		public ConnectionBuffer(int capacity) => _buffer = new byte[capacity];

		public int Capacity => _buffer.Length;
		public int Count => _count;
		public byte[] Buffer => _buffer;
		public int ReadIndex => _readIndex;

		public void Write(byte[] writeableData)
		{
			int requestedCount = writeableData.Length;
			int availableCount = Capacity - _count;
			if (requestedCount > availableCount)
				throw new Exception("The length of data buffer is bigger than available writeable space.");

			int maxRightSize = Capacity - _writeIndex;
			if (maxRightSize >= requestedCount)
			{
				Array.Copy(writeableData, 0, _buffer, _writeIndex, requestedCount);
				_writeIndex += requestedCount;
			}
			else
			{
				Array.Copy(writeableData, 0, _buffer, _writeIndex, maxRightSize);
				int residualAmount = requestedCount - maxRightSize;
				Array.Copy(writeableData, maxRightSize, _buffer, 0, residualAmount);
				_writeIndex = residualAmount;
			}
			_count += requestedCount;
		}

		public byte[] Read(int count)
		{
			if (count > _count)
				throw new Exception("Requested count of bytes less then available readable space.");
			byte[] bytes = new byte[count];
			int maxRightSize = Capacity - _readIndex;
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
			return bytes;
		}

		public ConnectionBuffer Copy() => new ConnectionBuffer(_buffer.Length) { _count = _count, _buffer = _buffer, _readIndex = _readIndex, _writeIndex = _writeIndex };
	}
}