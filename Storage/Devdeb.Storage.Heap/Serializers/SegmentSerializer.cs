using Devdeb.Serialization;

namespace Devdeb.Storage.Heap.Serializers
{
    public sealed class SegmentSerializer : ConstantLengthSerializer<Segment>
    {
        static public SegmentSerializer Default { get; } = new SegmentSerializer();

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
