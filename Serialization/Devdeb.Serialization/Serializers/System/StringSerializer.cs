using System;
using System.Text;

namespace Devdeb.Serialization.Serializers.System
{
	public sealed class StringSerializer : Serializer<string>
	{
		static StringSerializer()
		{
			Default = new StringSerializer(Encoding.Default);
			ASCII = new StringSerializer(Encoding.ASCII);
			UTF7 = new StringSerializer(Encoding.UTF7);
			UTF8 = new StringSerializer(Encoding.UTF8);
			UTF32 = new StringSerializer(Encoding.UTF32);
			Unicode = new StringSerializer(Encoding.Unicode);
			BigEndianUnicode = new StringSerializer(Encoding.BigEndianUnicode);
		}

		static public StringSerializer Default { get; }
		static public StringSerializer ASCII { get; }
		static public StringSerializer UTF7 { get; }
		static public StringSerializer UTF8 { get; }
		static public StringSerializer UTF32 { get; }
		static public StringSerializer Unicode { get; }
		static public StringSerializer BigEndianUnicode { get; }

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
