using Devdeb.Serialization.Serializers.System.BigEndian;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class Int16Serializer : IConstantLengthSerializer<short>
    {
        static public Int16Serializer Default { get; } = new Int16Serializer();
        static public BigEndianInt16Serializer BigEndian { get; } = BigEndianInt16Serializer.Default;

        public int Size => sizeof(short);

        public unsafe void Serialize(short instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(short*)bufferPointer = instance;
        }
        public unsafe short Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(short*)bufferPointer;
        }
    }
}
