using NAudio.Wave;
using System;
using System.Collections.Concurrent;

namespace Devdeb.Audio.InternetTelephony.Client.Audio
{
	public class NetworkWaveProvider : IWaveProvider
	{
		private int _currentBufferReadIndex;
		private readonly ConcurrentQueue<byte[]> _buffers;

		public NetworkWaveProvider()
		{
			_buffers = new ConcurrentQueue<byte[]>();
			_currentBufferReadIndex = 0;
		}

		public WaveFormat WaveFormat => new WaveFormat(44800, 16, 2);

		public int Read(byte[] buffer, int offset, int count)
		{
			int needRead = count;
			for (; count != 0 && _buffers.TryPeek(out byte[] currentBuffer); )
			{
				var remainsReadBytes = currentBuffer.Length - _currentBufferReadIndex;
				if (remainsReadBytes == 0)
				{ 
					_ = _buffers.TryDequeue(out _);
					_currentBufferReadIndex = 0;
					continue;
				}

				var currentBufferReadBytes = Math.Min(remainsReadBytes, count);
				Array.Copy(currentBuffer, _currentBufferReadIndex, buffer, offset, currentBufferReadBytes);
				offset += currentBufferReadBytes;
				count -= currentBufferReadBytes;
				_currentBufferReadIndex += currentBufferReadBytes;
			}

			if (count != 0)
				Array.Fill<byte>(buffer, 0, offset, count);
			
			return needRead;
		}

		public void AddBuffer(byte[] buffer) => _buffers.Enqueue(buffer);
	}
}
