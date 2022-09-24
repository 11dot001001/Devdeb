using Devdeb.Serialization.Serializers.System.BigEndian;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class Int64Serializer : IConstantLengthSerializer<long>
    {
        static public Int64Serializer Default { get; } = new Int64Serializer();
        static public BigEndianInt64Serializer BigEndian { get; } = BigEndianInt64Serializer.Default;

        public int Size => sizeof(long);

        public unsafe void Serialize(long instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(long*)bufferPointer = instance;
        }
        public unsafe long Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(long*)bufferPointer;
        }
    }
}
