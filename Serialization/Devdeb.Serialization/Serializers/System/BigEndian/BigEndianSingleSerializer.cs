using System;

namespace Devdeb.Serialization.Serializers.System.BigEndian
{
	public class BigEndianSingleSerializer : IConstantLengthSerializer<float>
	{
		static public BigEndianSingleSerializer Default { get; } = new BigEndianSingleSerializer();

        public int Size => sizeof(float);

        public unsafe void Serialize(float instance, Span<byte> buffer)
		{
			byte* instancePointer = (byte*)&instance;

			buffer[0] = *(instancePointer + 3);
			buffer[1] = *(instancePointer + 2);
			buffer[2] = *(instancePointer + 1);
			buffer[3] = *instancePointer;
		}
        public unsafe float Deserialize(ReadOnlySpan<byte> buffer)
		{
			float instance;
			byte* instancePointer = (byte*)&instance;
			*(instancePointer + 3) = buffer[0];
			*(instancePointer + 2) = buffer[1];
			*(instancePointer + 1) = buffer[2];
			*instancePointer = buffer[3];
			return instance;
		}
    }
}
