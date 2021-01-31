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
			FieldInfo[] fields = members.Where(x => x is FieldInfo).Select(x => x as FieldInfo).ToArray();
			PropertyInfo[] properties = members.Where(x => x is PropertyInfo).Select(x => x as PropertyInfo).Where(x => x.CanRead && x.CanWrite).ToArray();

			SerializerBuilder<T> serializerBuilder = new SerializerBuilder<T>();
			for (int i = 0; i != fields.Length; i++)
				serializerBuilder.AddMember(fields[i]);
			for (int i = 0; i != properties.Length; i++)
				serializerBuilder.AddMember(properties[i]);

			DefaultSerializersStorage.AddSerializer(serializationType, _serializer = serializerBuilder.Build());
		}

		static public ISerializer<T> Instance => _serializer;
	}
}
