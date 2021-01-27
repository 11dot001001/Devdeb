using Devdeb.Serialization.Serializers.System;
using System;

namespace Devdeb.Serialization.Serializers
{
	public sealed class StringLengthSerializer : Serializer<string>
	{
		static StringLengthSerializer()
		{
			Default = new StringLengthSerializer(StringSerializer.Default);
			ASCII = new StringLengthSerializer(StringSerializer.ASCII);
			UTF7 = new StringLengthSerializer(StringSerializer.UTF7);
			UTF8 = new StringLengthSerializer(StringSerializer.UTF8);
			UTF32 = new StringLengthSerializer(StringSerializer.UTF32);
			Unicode = new StringLengthSerializer(StringSerializer.Unicode);
			BigEndianUnicode = new StringLengthSerializer(StringSerializer.BigEndianUnicode);
		}

		static public StringLengthSerializer Default { get; }
		static public StringLengthSerializer ASCII { get; }
		static public StringLengthSerializer UTF7 { get; }
		static public StringLengthSerializer UTF8 { get; }
		static public StringLengthSerializer UTF32 { get; }
		static public StringLengthSerializer Unicode { get; }
		static public StringLengthSerializer BigEndianUnicode { get; }

		private readonly StringSerializer _stringSerializer;
		private readonly Int32Serializer _int32Serializer;

		public StringLengthSerializer(StringSerializer stringSerializer)
		{
			_stringSerializer = stringSerializer ?? throw new ArgumentNullException(nameof(stringSerializer));
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
		public override string Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			int stringSize = _int32Serializer.Deserialize(buffer, ref offset);
			if (stringSize == 0)
				return string.Empty;
			return _stringSerializer.Deserialize(buffer, offset, stringSize);
		}
	}
}
