using System;

namespace Devdeb.Serialization.Serializers.System
{
	public sealed class NullableSerializer<T> : Serializer<T>
	{
		private readonly ISerializer<T> _elementSerializer;
		private readonly BooleanSerializer _booleanSerializer;

		public NullableSerializer(ISerializer<T> elementSerializer) : base(SerializerFlags.NullInstance)
		{
			_elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
			_booleanSerializer = new BooleanSerializer();
		}

		public override int Size(T instance)
		{
			VerifySize(instance);
			int size = _booleanSerializer.Size;
			if (instance != null)
				size += _elementSerializer.Size(instance);
			return size;
		}
		public override void Serialize(T instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			bool hasValue = instance != null;
			_booleanSerializer.Serialize(hasValue, buffer, ref offset);
			if (!hasValue)
				return;
			_elementSerializer.Serialize(instance, buffer, offset);
		}
		public override T Deserialize(byte[] buffer, int offset, int? count)
		{
			VerifyDeserialize(buffer, offset, count);
			bool hasValue = _booleanSerializer.Deserialize(buffer, ref offset);
			if (!hasValue)
				return default;
			return _elementSerializer.Deserialize(buffer, offset);
		}
	}
}
