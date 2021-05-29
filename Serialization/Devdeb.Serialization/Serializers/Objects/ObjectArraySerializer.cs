using System;
using System.Linq;

namespace Devdeb.Serialization.Serializers.Objects
{
	public class ObjectArraySerializer : Serializer<object[]>
	{
		private readonly ObjectSerializer[] _objectSerializers;

		public ObjectArraySerializer(params ObjectSerializer[] objectSerializers)
		{
			_objectSerializers = objectSerializers ?? throw new ArgumentNullException(nameof(objectSerializers));
			if (objectSerializers.Any(x => x.Flags.HasFlag(SerializerFlags.NeedCount)))
			{
				throw new Exception("The array serializer cannot deal with need count serializers.");
			}
		}

		public override int Size(object[] instance)
		{
			VerifySize(instance);
			int size = 0;
			for (int i = 0; i != instance.Length; i++)
				size = checked(size + _objectSerializers[i].Size(instance[i]));
			return size;
		}
		public override void Serialize(object[] instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			for (int i = 0; i != instance.Length; i++)
				_objectSerializers[i].Serialize(instance[i], buffer, ref offset);
		}
		public override object[] Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			object[] instance = new object[_objectSerializers.Length];
			for (int i = 0; i != instance.Length; i++)
				instance[i] = _objectSerializers[i].Deserialize(buffer, ref offset, null);
			return instance;
		}
	}
}
