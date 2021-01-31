using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Serialization.Default
{
	static internal class DefaultSerializersStorage
	{
		static private readonly Dictionary<Type, object> _defaultSerializers;

		static DefaultSerializersStorage() => _defaultSerializers = new Dictionary<Type, object>
		{
			[typeof(bool)] = BooleanSerializer.Default,
			[typeof(byte)] = ByteSerializer.Default,
			[typeof(char)] = CharSerializer.Default,
			[typeof(DateTime)] = DateTimeSerializer.Default,
			[typeof(decimal)] = DecimalSerializer.Default,
			[typeof(double)] = DoubleSerializer.Default,
			[typeof(Guid)] = GuidSerializer.Default,
			[typeof(short)] = Int16Serializer.Default,
			[typeof(int)] = Int32Serializer.Default,
			[typeof(long)] = Int64Serializer.Default,
			[typeof(sbyte)] = SByteSerializer.Default,
			[typeof(float)] = SingleSerializer.Default,
			[typeof(string)] = StringLengthSerializer.Default,
			[typeof(TimeSpan)] = TimeSpanSerializer.Default,
			[typeof(ushort)] = Int16Serializer.Default,
			[typeof(uint)] = UInt32Serializer.Default,
			[typeof(ulong)] = UInt64Serializer.Default
		};

		static public ISerializer<T> GetSerializer<T>()
		{
			if (!TryGetSerializer(typeof(T), out object serializerObject))
				throw new Exception($"Serializer wasn't found by type {typeof(T)}.");
			return (ISerializer<T>)serializerObject;
		}
		static public bool TryGetSerializer<T>(out ISerializer<T> serializer)
		{
			bool wasFound = TryGetSerializer(typeof(T), out object serializerObject);
			serializer = wasFound ? (ISerializer<T>)serializerObject : default;
			return wasFound;
		}
		static public bool TryGetSerializer(Type type, out object serializer)
		{
			return _defaultSerializers.TryGetValue(type, out serializer);
		}
		static public void AddSerializer<T>(ISerializer<T> serializer)
		{
			_defaultSerializers.Add(typeof(T), serializer);
		}
		static public void AddSerializer(Type type, object serializer)
		{
			Type serializerInterface = serializer.GetType()
												 .GetInterfaces()
												 .FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(ISerializer<>));
			if (serializerInterface == null)
				throw new Exception($"The {serializer} doesn't implement {nameof(ISerializer<object>)} type.");

			Type serializationObject = serializerInterface.GetGenericArguments().First();
			if (serializationObject != type)
				throw new Exception($"The type serialized by {serializer} is not equal to {type}.");

			_defaultSerializers.Add(type, serializer);
		}
	}
}
