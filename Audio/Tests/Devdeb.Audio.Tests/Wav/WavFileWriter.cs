using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Devdeb.Audio.Tests.Wav
{
	public static class WavFileWriter
	{
		public static void WriteFile(WavFile file, string path)
		{
			using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

			fileStream.Write(file.ChunkId);
			fileStream.Write(ConvertToSpan(file.ChunkSize));
			fileStream.Write(file.Format);
			fileStream.Write(file.Subchunk1ID);
			fileStream.Write(ConvertToSpan(file.Subchunk1Size));
			fileStream.Write(ConvertToSpan((short)file.AudioFormat));
			fileStream.Write(ConvertToSpan((short)file.NumChannels));
			fileStream.Write(ConvertToSpan(file.SampleRate));
			fileStream.Write(ConvertToSpan(file.ByteRate));
			fileStream.Write(ConvertToSpan(file.BlockAlign));
			fileStream.Write(ConvertToSpan(file.BitsPerSample));
			fileStream.Write(file.Subchunk2ID);
			fileStream.Write(ConvertToSpan(file.Subchunk2Size));
			fileStream.Write(file.Data);
		}

		private static unsafe ReadOnlySpan<byte> ConvertToSpan<T>(T value) where T : unmanaged
		{
			byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];

			fixed (byte* bytesPointer = &bytes[0])
				*(T*)bytesPointer = value;

			return bytes.AsSpan();
		}
	}
}
