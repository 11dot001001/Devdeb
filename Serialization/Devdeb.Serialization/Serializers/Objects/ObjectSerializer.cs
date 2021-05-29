using Devdeb.Serialization.Default;
using System;
using System.Reflection;

namespace Devdeb.Serialization.Serializers.Objects
{
	public class ObjectSerializer : Serializer<object>
	{
		static private object GetDefaultSerializer(Type type)
		{
			return typeof(DefaultSerializer<>)
					.MakeGenericType(type)
					.GetProperty(nameof(DefaultSerializer<object>.Instance))
					.GetGetMethod()
					.Invoke(null, null);
		}

		private readonly object _serializer;
		private readonly Type _serializerType;
		private readonly Type _objectType;
		private readonly MethodInfo _sizeMethod;
		private readonly MethodInfo _serializeMethod;
		private readonly MethodInfo _deserializeMethod;

		public ObjectSerializer(Type objectType, SerializerFlags flags = SerializerFlags.Empty)
			: this(objectType, GetDefaultSerializer(objectType), flags) { }

		public ObjectSerializer(Type objectType, object objectSerializer, SerializerFlags flags = SerializerFlags.Empty) : base(flags)
		{
			_objectType = objectType;
			_serializer = objectSerializer;
			_serializerType = _serializer.GetType();

			Type serializerInterfaceType = typeof(ISerializer<>).MakeGenericType(_objectType);
			InterfaceMapping serializerInterfaceMapping = _serializerType.GetInterfaceMap(serializerInterfaceType);

			for (int i = 0; i < serializerInterfaceMapping.InterfaceMethods.Length; i++)
			{
				if (serializerInterfaceMapping.InterfaceMethods[i] == serializerInterfaceType.GetMethod(nameof(ISerializer<object>.Size)))
				{
					_sizeMethod = serializerInterfaceMapping.TargetMethods[i];
					continue;
				}
				if (serializerInterfaceMapping.InterfaceMethods[i] == serializerInterfaceType.GetMethod(nameof(ISerializer<object>.Serialize), new[] { _objectType, typeof(byte[]), typeof(int) }))
				{
					_serializeMethod = serializerInterfaceMapping.TargetMethods[i];
					continue;
				}
				if (serializerInterfaceMapping.InterfaceMethods[i] == serializerInterfaceType.GetMethod(nameof(ISerializer<object>.Deserialize), new[] { typeof(byte[]), typeof(int), typeof(int?) }))
				{
					_deserializeMethod = serializerInterfaceMapping.TargetMethods[i];
					continue;
				}
			}
		}

		public override int Size(object instance)
		{
			VerifySize(instance);
			return (int)_sizeMethod.Invoke(_serializer, new[] { instance });
		}
		public override void Serialize(object instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_serializeMethod.Invoke(_serializer, new[] { instance, buffer, offset });
		}
		public override object Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			return _deserializeMethod.Invoke(_serializer, new object[] { buffer, offset, count });
		}
	}
}
