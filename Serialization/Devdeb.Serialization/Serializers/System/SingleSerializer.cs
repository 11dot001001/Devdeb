using Devdeb.Serialization.Serializers.System.BigEndian;
using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class SingleSerializer : IConstantLengthSerializer<float>
    {
        static public SingleSerializer Default { get; } = new SingleSerializer();
        static public BigEndianSingleSerializer BigEndian { get; } = BigEndianSingleSerializer.Default;

        public int Size => sizeof(float);

        public unsafe void Serialize(float instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(float*)bufferPointer = instance;
        }
        public unsafe float Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(float*)bufferPointer;
        }
    }
}
