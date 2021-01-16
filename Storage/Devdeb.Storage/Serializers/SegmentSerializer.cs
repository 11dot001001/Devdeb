namespace Devdeb.Storage.Serializers
{
	public class SegmentSerializer
	{
		public int Size() => sizeof(long) + sizeof(long);

		public unsafe void Serialize(Segment instance, byte[] buffer, int offset)
		{
			fixed (byte* bufferPointer = &buffer[offset])
			{
				*(long*)bufferPointer = instance.Pointer;
				*((long*)bufferPointer + 1) = instance.Size;
			}
		}
		public unsafe Segment Deserialize(byte[] buffer, int offset)
		{
			Segment instance = new Segment();
			fixed (byte* bufferPointer = &buffer[offset])
			{
				instance.Pointer = *(long*)bufferPointer;
				instance.Size = *((long*)bufferPointer + 1);
			}
			return instance;
		}
	}
}
