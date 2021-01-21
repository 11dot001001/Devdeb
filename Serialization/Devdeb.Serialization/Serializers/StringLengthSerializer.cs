using Devdeb.Serialization.Serializers.System;
using System;
using System.Text;

namespace Devdeb.Serialization.Serializers
{
	public class StringLengthSerializer : Serializer<string>
	{
		private readonly StringSerializer _stringSerializer;
		private readonly Int32Serializer _int32Serializer;

		public StringLengthSerializer(Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException(nameof(encoding));
			_stringSerializer = new StringSerializer(encoding);
			_int32Serializer = new Int32Serializer();
		}

		public override int Size(string instance)
		{
			VerifySize(instance);
			return _int32Serializer.Size + _stringSerializer.Size(instance);
		}
		public override void Serialize(string instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			int stringSize = _stringSerializer.Size(instance);
			_int32Serializer.Serialize(stringSize, buffer, ref offset);
			if (stringSize == 0)
				return;
			_stringSerializer.Serialize(instance, buffer, offset);
		}
		public override string Deserialize(byte[] buffer, int offset, int? count)
		{
			VerifyDeserialize(buffer, offset, count);
			int stringSize = _int32Serializer.Deserialize(buffer, ref offset);
			if (stringSize == 0)
				return string.Empty;
			return _stringSerializer.Deserialize(buffer, offset, stringSize);
		}
	}
}
