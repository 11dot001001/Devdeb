using System;

namespace Devdeb.Serialization.Serializers.System
{
    public sealed class DoubleSerializer : IConstantLengthSerializer<double>
    {
        static public DoubleSerializer Default { get; } = new DoubleSerializer();

        public int Size => sizeof(double);

        public unsafe void Serialize(double instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(double*)bufferPointer = instance;
        }
        public unsafe double Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(double*)bufferPointer;
        }
    }
}
