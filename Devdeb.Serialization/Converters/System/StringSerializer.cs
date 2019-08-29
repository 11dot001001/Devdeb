using System;
using System.Text;

namespace Devdeb.Serialization.Converters.System
{
	public sealed class StringSerializer : Serializer<string>
	{
		public override int GetBytesCount(string instance) => IntegerSerializer.BytesCount + Encoding.UTF8.GetByteCount(instance);
		public unsafe override void Serialize(string instance, byte[] buffer, ref int index)
		{
			int instanceLength = Encoding.UTF8.GetByteCount(instance);
			byte[] bytes = Encoding.UTF8.GetBytes(instance);

			fixed (byte* indexAddress = &buffer[index])
				*(int*)indexAddress = instanceLength;
			index += IntegerSerializer.BytesCount;
			Array.Copy(bytes, 0, buffer,  index, instanceLength);
			index += instanceLength;
		}
		public unsafe override string Deserialize(byte[] buffer, ref int index)
		{
			int instanceLength = 0;
			fixed (byte* bufferAddress = &buffer[index])
				instanceLength = *(int*)bufferAddress;
			index += IntegerSerializer.BytesCount;
			byte[] value = new byte[instanceLength];
			Array.Copy(buffer, index, value, 0, instanceLength);
			index += instanceLength;
			return Encoding.UTF8.GetString(value);
		}
	}
}