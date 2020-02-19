using System;

namespace Devdeb.Sets
{
	public class RingBuffer
	{
		private const int DefaultBufferCapacity = 1024;

		private readonly byte[] _data;
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
			if (requestedCount >= availableCount)
				throw new Exception("The length of data buffer is bigger than available writeable space.");
			if (_writeIndex >= _readIndex)
			{
				int maxRightSize = Capacity - _writeIndex;
				if (maxRightSize >= requestedCount)
					Array.Copy(writeableData, 0, _data, _writeIndex += requestedCount, requestedCount);
				else
				{
					Array.Copy(writeableData, 0, _data, _writeIndex, maxRightSize);
					int residualAmount = requestedCount - maxRightSize;
					Array.Copy(writeableData, maxRightSize, _data, 0, residualAmount);
					_writeIndex = residualAmount;
				}
			}
			else
				Array.Copy(writeableData, 0, _data, _writeIndex += requestedCount, requestedCount);
			_count -= requestedCount;
		}

		public byte[] Read() => throw new NotImplementedException();
	}
}