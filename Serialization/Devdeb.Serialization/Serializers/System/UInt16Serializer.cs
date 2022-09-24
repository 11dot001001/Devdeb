using Devdeb.Serialization.Serializers.System.BigEndian;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class UInt16Serializer : IConstantLengthSerializer<ushort>
    {
        static public UInt16Serializer Default { get; } = new UInt16Serializer();
        static public BigEndianUInt16Serializer BigEndian { get; } = BigEndianUInt16Serializer.Default;

        public int Size => sizeof(ushort);

        public unsafe void Serialize(ushort instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(ushort*)bufferPointer = instance;
        }
        public unsafe ushort Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(ushort*)bufferPointer;
        }
    }
}
