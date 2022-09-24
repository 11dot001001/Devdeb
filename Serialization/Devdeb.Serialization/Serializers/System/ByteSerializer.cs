using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class ByteSerializer : IConstantLengthSerializer<byte>
    {
        static public ByteSerializer Default { get; } = new ByteSerializer();

        public int Size => sizeof(byte);

        public void Serialize(byte instance, Span<byte> buffer) => buffer[0] = instance;
        public byte Deserialize(ReadOnlySpan<byte> buffer) => buffer[0];
    }
}
