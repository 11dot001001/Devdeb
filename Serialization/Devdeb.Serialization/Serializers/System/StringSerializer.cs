using System;
using System.Text;

namespace Devdeb.Serialization.Serializers.System
{
	public class StringSerializer : Serializer<string>
	{
		private readonly Encoding _encoding;

		public StringSerializer(Encoding encoding) : base(SerializerFlags.NeedCount)
		{
			_encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
		}

		public override int Size(string instance)
		{
			VerifySize(instance);
			return _encoding.GetByteCount(instance);
		}

		public override void Serialize(string instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			int writtenByteCount = _encoding.GetBytes(instance, 0, instance.Length, buffer, offset);
			if (writtenByteCount != Size(instance))
				throw new Exception($"The {nameof(writtenByteCount)} doesn't match the {nameof(instance)} size.");
		}
		public override string Deserialize(byte[] buffer, int offset, int? count)
		{
			VerifyDeserialize(buffer, offset, count);
			if (!count.HasValue)
				throw new ArgumentNullException(nameof(count));
			return _encoding.GetString(buffer, offset, count.Value);
		}
	}
}
