using System;

namespace Devdeb.Serialization.Serializers.System.BigEndian
{
    public sealed class BigEndianUInt16Serializer : IConstantLengthSerializer<ushort>
    {
        static public BigEndianUInt16Serializer Default { get; } = new BigEndianUInt16Serializer();

        public int Size => sizeof(ushort);

        public unsafe void Serialize(ushort instance, Span<byte> buffer)
        {
            byte* instancePointer = (byte*)&instance;

            buffer[0] = *(instancePointer + 1);
            buffer[1] = *instancePointer;
        }
        public unsafe ushort Deserialize(ReadOnlySpan<byte> buffer)
        {
            ushort instance;
            byte* instancePointer = (byte*)&instance;
            *(instancePointer + 1) = buffer[0];
            *instancePointer = buffer[1];
            return instance;
        }
    }
}
