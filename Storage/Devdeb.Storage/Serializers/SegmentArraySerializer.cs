namespace Devdeb.Storage.Serializers
{
	public class SegmentArraySerializer
	{
		public const int ArrayLengthSize = sizeof(int);

		static private SegmentSerializer _segmentSerializer = new SegmentSerializer();

		static public int ElementSize => _segmentSerializer.Size();

		public unsafe byte[] Serialize(Segment[] segments)
		{
			byte[] buffer = new byte[ArrayLengthSize + segments.Length * _segmentSerializer.Size()];
			fixed (byte* bufferPointer = &buffer[0])
				*(int*)bufferPointer = segments.Length;
			int offset = ArrayLengthSize;
			for (int i = 0; i != segments.Length; i++)
			{
				_segmentSerializer.Serialize(segments[i], buffer, offset);
				offset += _segmentSerializer.Size();
			}
			return buffer;
		}
		public unsafe Segment[] Deserialize(byte[] buffer)
		{
			int arrayLength;
			fixed (byte* bufferPointer = &buffer[0])
				arrayLength = *(int*)bufferPointer;

			Segment[] segments = new Segment[arrayLength];
			int offset = ArrayLengthSize;
			for (int i = 0; i != segments.Length; i++)
			{
				segments[i] = _segmentSerializer.Deserialize(buffer, offset);
				offset += _segmentSerializer.Size();
			}
			return segments;
		}
	}
}
