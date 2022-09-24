using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class SByteSerializer : IConstantLengthSerializer<sbyte>
    {
        static public SByteSerializer Default { get; } = new SByteSerializer();

        public int Size => sizeof(sbyte);

        public void Serialize(sbyte instance, Span<byte> buffer) => buffer[0] = (byte)instance;
        public sbyte Deserialize(ReadOnlySpan<byte> buffer) => (sbyte)buffer[0];
    }
}
