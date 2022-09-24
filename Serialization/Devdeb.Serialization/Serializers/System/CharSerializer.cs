using System;

namespace Devdeb.Serialization.Serializers.System
{
    public class CharSerializer : IConstantLengthSerializer<char>
    {
        static public CharSerializer Default { get; } = new CharSerializer();

        public int Size => sizeof(char);

        public unsafe void Serialize(char instance, Span<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                *(char*)bufferPointer = instance;
        }
        public unsafe char Deserialize(ReadOnlySpan<byte> buffer)
        {
            fixed (byte* bufferPointer = buffer)
                return *(char*)bufferPointer;
        }
    }
}
