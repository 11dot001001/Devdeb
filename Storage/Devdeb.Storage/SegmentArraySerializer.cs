namespace Devdeb.Storage
{
	public class SegmentArraySerializer
	{
		public const int ArrayLengthBytesCount = sizeof(int);
		public const int SegmentLengthBytesCount = sizeof(long) + sizeof(long);

		public unsafe byte[] Serialize(Segment[] segments)
		{
			byte[] buffer = new byte[ArrayLengthBytesCount + segments.Length * SegmentLengthBytesCount];
			fixed (byte* bufferPointer = &buffer[0])
			{
				*(int*)bufferPointer = segments.Length;
				for (int i = 0; i != segments.Length; i++)
				{
					long* currentPointer = (long*)(bufferPointer + ArrayLengthBytesCount) + (i * 2);
					*currentPointer = segments[i].Pointer;
					*(currentPointer + 1) = segments[i].Size;
				}
			}
			return buffer;
		}
		public unsafe Segment[] Deserialize(byte[] buffer)
		{
			int arrayLength;
			fixed (byte* bufferPointer = &buffer[0])
				arrayLength = *(int*)bufferPointer;

			Segment[] segments = new Segment[arrayLength];
			fixed (byte* bufferPointer = &buffer[0])
			{
				for (int i = 0; i != segments.Length; i++)
				{
					long* currentPointer = (long*)(bufferPointer + ArrayLengthBytesCount) + (i * 2);
					segments[i] = new Segment
					{
						Pointer = *currentPointer,
						Size = *(currentPointer + 1)
					};
				}
			}
			return segments;
		}
	}
}
