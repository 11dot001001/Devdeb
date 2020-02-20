using Devdeb.Sets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Devdeb.Tests.Sets
{
	[TestClass]
	public class RingBufferTester
	{
		private enum Method
		{
			Write,
			Read
		}

		private readonly int _restartCount = int.MaxValue / 2;
		private readonly RingBuffer _ringBuffer = new RingBuffer();
		private readonly Random _random = new Random();

		[TestMethod]
		public void TestMethod1()
		{
			for (int i = 0; i < _restartCount; i++)
			{
				Method method = (Method)_random.Next(0, 2);
				switch (method)
				{
					case Method.Write:
					{
						int availableSpace = _ringBuffer.Capacity - _ringBuffer.Count;
						if (availableSpace == 0)
							continue;
						byte[] bytes = new byte[_random.Next(1, availableSpace + 1)];
						_ringBuffer.Write(bytes);
						break;
					}
					case Method.Read:
					{
						byte[] bytes = _ringBuffer.Read(_random.Next(0, _ringBuffer.Count + 1));
						break;
					}
					default:
						throw new Exception();
				}
			}
		}
	}
}