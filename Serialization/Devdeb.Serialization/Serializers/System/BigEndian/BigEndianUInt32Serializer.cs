using System;

namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public class BigEndianUInt32Serializer : IConstantLengthSerializer<uint>
	{
		static public BigEndianUInt32Serializer Default { get; } = new BigEndianUInt32Serializer();

		public int Size => sizeof(uint);

        public unsafe void Serialize(uint instance, Span<byte> buffer)
		{
			byte* instancePointer = (byte*)&instance;

			buffer[0] = *(instancePointer + 3);
			buffer[1] = *(instancePointer + 2);
			buffer[2] = *(instancePointer + 1);
			buffer[3] = *instancePointer;
		}
        public unsafe uint Deserialize(ReadOnlySpan<byte> buffer)
		{
			uint instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 3) = buffer[0];
			*(instancePointer + 2) = buffer[1];
			*(instancePointer + 1) = buffer[2];
			*instancePointer = buffer[3];
			return instance;
		}
    }
}
