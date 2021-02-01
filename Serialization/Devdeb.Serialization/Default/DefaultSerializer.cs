using Devdeb.Serialization.Builders;
using System;
using System.Linq;
using System.Reflection;

namespace Devdeb.Serialization.Default
{
	static public class DefaultSerializer<T>
	{
		static private readonly ISerializer<T> _serializer;

		static DefaultSerializer()
		{
			Type serializationType = typeof(T);
			if (DefaultSerializersStorage.TryGetSerializer(serializationType, out object serializer))
			{
				_serializer = (ISerializer<T>)serializer;
				return;
			}

			MemberInfo[] members = serializationType.GetMembers(BindingFlags.Public | BindingFlags.Instance);
			
			SerializerBuilder<T> serializerBuilder = new SerializerBuilder<T>();
			foreach (MemberInfo memberInfo in members.Where(x => SerializerBuilder<T>.MemberSelectionPredicate(x)))
				serializerBuilder.AddMember(memberInfo);
			DefaultSerializersStorage.AddSerializer(serializationType, _serializer = serializerBuilder.Build());
		}

		static public ISerializer<T> Instance => _serializer;
	}
}
