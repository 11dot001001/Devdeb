using System;

namespace Devdeb.Serialization.Serializers.System.Collections
{
	public class ArraySerializer<T> : Serializer<T[]>
	{
		private readonly ISerializer<T> _elementSerializer;

		public ArraySerializer(ISerializer<T> elementSerializer) : base(SerializerFlags.NeedCount)
		{
			_elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
		}

		public override int Size(T[] instance)
		{
			VerifySize(instance);
			int size = 0;
			for (int i = 0; i != instance.Length; i++)
				size += _elementSerializer.Size(instance[i]);
			return size;
		}
		public override void Serialize(T[] instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			for (int i = 0; i != instance.Length; i++)
				_elementSerializer.Serialize(instance[i], buffer, ref offset);
		}
		public override T[] Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			T[] instance = new T[count.Value];
			for (int i = 0; i != instance.Length; i++)
				instance[i] = _elementSerializer.Deserialize(buffer, ref offset, null);
			return instance;
		}
	}
}
