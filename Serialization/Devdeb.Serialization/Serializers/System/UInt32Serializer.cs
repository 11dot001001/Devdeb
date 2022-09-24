using Devdeb.Serialization.Serializers.System.BigEndian;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class UInt32Serializer : IConstantLengthSerializer<uint>
    {
        static public UInt32Serializer Default { get; } = new UInt32Serializer();
        static public BigEndianUInt32Serializer BigEndian { get; } = BigEndianUInt32Serializer.Default;

        public int Size => sizeof(uint);

        public unsafe void Serialize(uint instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(uint*)bufferPointer = instance;
        }

        public unsafe uint Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(uint*)bufferPointer;
        }
    }
}
