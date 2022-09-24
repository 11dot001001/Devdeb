using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class BooleanSerializer : IConstantLengthSerializer<bool>
    {
        static public BooleanSerializer Default { get; } = new BooleanSerializer();

        public int Size => sizeof(bool);

        public unsafe void Serialize(bool instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(bool*)bufferPointer = instance;
        }
        public unsafe bool Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(bool*)bufferPointer;
        }
    }
}
