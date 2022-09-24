using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class DecimalSerializer : IConstantLengthSerializer<decimal>
    {
        static public DecimalSerializer Default { get; } = new DecimalSerializer();

        public int Size => sizeof(decimal);

        public unsafe void Serialize(decimal instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(decimal*)bufferPointer = instance;
        }
        public unsafe decimal Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(decimal*)bufferPointer;
        }
    }
}
