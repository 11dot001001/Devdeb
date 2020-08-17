using System;

namespace Devdeb.Sets
{
	public class RingBuffer
	{
		private const int DefaultBufferCapacity = 1024;

		private byte[] _data;
		private int _readIndex;
		private int _writeIndex;
		private int _count;

		public RingBuffer() : this(DefaultBufferCapacity) { }
		public RingBuffer(int capacity) => _data = new byte[capacity];

		public int Capacity => _data.Length;
		public int Count => _count;

		public void Write(byte[] writeableData)
		{
			int requestedCount = writeableData.Length;
			int availableCount = Capacity - _count;
			if (requestedCount > availableCount)
				throw new Exception("The length of data buffer is bigger than available writeable space.");

			int maxRightSize = Capacity - _writeIndex;
			if (maxRightSize >= requestedCount)
			{
				Array.Copy(writeableData, 0, _data, _writeIndex, requestedCount);
				_writeIndex += requestedCount;
			}
			else
			{
				Array.Copy(writeableData, 0, _data, _writeIndex, maxRightSize);
				int residualAmount = requestedCount - maxRightSize;
				Array.Copy(writeableData, maxRightSize, _data, 0, residualAmount);
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
				Array.Copy(_data, _readIndex, bytes, 0, count);
				_readIndex += count;
			}
			else
			{
				Array.Copy(_data, _readIndex, bytes, 0, maxRightSize);
				int residualAmount = count - maxRightSize;
				Array.Copy(_data, 0, bytes, maxRightSize, residualAmount);
				_readIndex = residualAmount;
			}
			_count -= count;
			return bytes;
		}

		public RingBuffer Copy()
		{
			return new RingBuffer(_data.Length) { _count = _count, _data = _data, _readIndex = _readIndex, _writeIndex = _writeIndex };
		}
	}
}