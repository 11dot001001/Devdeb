using Devdeb.Serialization.Serializers.System;
using Devdeb.Serialization.Serializers.System.Collections;
using System;

namespace Devdeb.Serialization.Serializers
{
	public class ArrayLengthSerializer<T> : Serializer<T[]>
	{
		private readonly ArraySerializer<T> _arraySerializer;
		private readonly Int32Serializer _int32Serializer;

		public ArrayLengthSerializer(ISerializer<T> elementSerializer)
		{
			if (elementSerializer == null)
				throw new ArgumentNullException(nameof(elementSerializer));
			_arraySerializer = new ArraySerializer<T>(elementSerializer);
			_int32Serializer = new Int32Serializer();
		}

		public override int Size(T[] instance)
		{
			VerifySize(instance);
			int size = _int32Serializer.Size;
			size += _arraySerializer.Size(instance);
			return size;
		}
		public override void Serialize(T[] instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_int32Serializer.Serialize(instance.Length, buffer, ref offset);
			_arraySerializer.Serialize(instance, buffer, offset);
		}
		public override T[] Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			int elementsLength = _int32Serializer.Deserialize(buffer, ref offset);
			return _arraySerializer.Deserialize(buffer, offset, elementsLength);
		}
	}
}
