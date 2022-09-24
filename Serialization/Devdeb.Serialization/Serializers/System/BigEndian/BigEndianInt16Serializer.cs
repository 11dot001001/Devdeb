using System;

namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public sealed class BigEndianInt16Serializer : IConstantLengthSerializer<short>
	{
		static public BigEndianInt16Serializer Default { get; } = new BigEndianInt16Serializer();

        public int Size => sizeof(short);

        public unsafe void Serialize(short instance, Span<byte> buffer)
		{
			byte* instancePointer = (byte*)&instance;

			buffer[0] = *(instancePointer + 1);
			buffer[1] = *instancePointer;
		}
        public unsafe short Deserialize(ReadOnlySpan<byte> buffer)
		{
			short instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 1) = buffer[0];
			*instancePointer = buffer[1];
			return instance;
		}
    }
}
