using System.Text;

namespace Devdeb.Audio.Tests.Wav
{
	public class WavFile
	{
		public const int FormatBytesCount = 4;
		public const int Subchunk1IDBytesCount = 4;
		public const int Subchunk1SizeBytesCount = 4;
		public const int SampleRateBytesCount = 4;
		public const int ByteRateBytesCount = 4;
		public const int BlockAlignBytesCount = 2;
		public const int BitsPerSampleBytesCount = 2;
		public const int Subchunk2IDBytesCount = 4;
		public const int Subchunk2SizeBytesCount = 4;

		/// <remarks>RIFF chank.</remarks>
		public byte[] ChunkId { get; } = Encoding.ASCII.GetBytes("RIFF");

		/// <remarks>RIFF chank.</remarks>
		public int ChunkSize =>
			FormatBytesCount +
			Subchunk1IDBytesCount +
			Subchunk1SizeBytesCount +
			Subchunk1Size +
			Subchunk2IDBytesCount +
			Subchunk2SizeBytesCount +
			Subchunk2Size;

		/// <remarks>RIFF chank.</remarks>
		public byte[] Format { get; } = Encoding.ASCII.GetBytes("WAVE");

		/// <remarks>Format chank.</remarks>
		public byte[] Subchunk1ID { get; } = Encoding.ASCII.GetBytes("fmt ");

		/// <remarks>Format chank.</remarks>
		public int Subchunk1Size =
			sizeof(AudioFormat) +
			sizeof(NumChannels) +
			SampleRateBytesCount +
			ByteRateBytesCount +
			BlockAlignBytesCount +
			BitsPerSampleBytesCount;

		/// <remarks>Format chank.</remarks>
		public AudioFormat AudioFormat { get; } = AudioFormat.Pcm;

		/// <remarks>Format chank.</remarks>
		public NumChannels NumChannels { get; set; }

		/// <remarks>Format chank.</remarks>
		public int SampleRate { get; set; }

		/// <remarks>Format chank.</remarks>
		public int ByteRate => SampleRate * BlockAlign;

		/// <remarks>Format chank.</remarks>
		public short BlockAlign => (short)((short)NumChannels * (BitsPerSample >> 3));

		/// <remarks>Format chank.</remarks>
		public short BitsPerSample { get; set; }

		/// <remarks>Format chank.</remarks>
		//could insert extra parameters => used for a format other than pcm

		/// <remarks>Format chank.</remarks>
		public byte[] Subchunk2ID { get; } = Encoding.ASCII.GetBytes("data");

		/// <remarks>Format chank. </remarks>
		public int Subchunk2Size => Data.Length;

		public byte[] Data { get; set; }
	}
}
