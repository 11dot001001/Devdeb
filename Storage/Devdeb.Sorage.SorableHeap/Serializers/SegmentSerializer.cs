using Devdeb.Serialization;

namespace Devdeb.Sorage.SorableHeap.Serializers
{
	public sealed class SegmentSerializer : ConstantLengthSerializer<Segment>
	{
		static SegmentSerializer() => Default = new SegmentSerializer();
		static public SegmentSerializer Default { get; }

		public SegmentSerializer() : base(sizeof(long) * 2) { }

		public unsafe override void Serialize(Segment instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
			{
				*(long*)bufferPointer = instance.Pointer;
				*((long*)bufferPointer + 1) = instance.Size;
			}
		}
		public unsafe override Segment Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
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
