using System;

namespace Devdeb.Serialization.Serializers.System
{
	public sealed class ConstantStringSerializer : ConstantLengthSerializer<string>
	{
		private readonly StringSerializer _serializer;
		private readonly int _bytesCount;

		public ConstantStringSerializer(StringSerializer stringSerializer, int bytesCount) : base(bytesCount)
		{
			_serializer = stringSerializer ?? throw new ArgumentNullException(nameof(stringSerializer));
			_bytesCount = bytesCount;
		}

		public override void Serialize(string instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			int instanceSize = _serializer.Size(instance);
			if (instanceSize != _bytesCount)
				throw new ArgumentException($"Instance size not equal to {_bytesCount}.", nameof(instance));

			_serializer.Serialize(instance, buffer, offset);
		}

		public override string Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return _serializer.Deserialize(buffer, offset, _bytesCount);
		}
	}
}
