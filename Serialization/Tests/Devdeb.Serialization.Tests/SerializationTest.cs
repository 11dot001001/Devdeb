using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Serialization.Serializers.System.Collections;
using System;

namespace Devdeb.Serialization.Tests
{
	internal class SerializationTest
	{
		public void Test()
		{
			//StringSerializer serializer = new StringSerializer(Encoding.Default);
			//string a = string.Empty;
			//byte[] buffer = new byte[serializer.Size(a)];
			//serializer.Serialize(a, buffer, 0);
			//string c = serializer.Deserialize(buffer, 0, buffer.Length);

			string[] strings = new[]
			{
				"String 1",
				"Strdsfgsdfging 2",
				"Str3",
				string.Empty,
				null
			};
			NullableSerializer<string> nullableStringSerializer = new(StringLengthSerializer.Default);
			ArrayLengthSerializer<string> arraySerializer = new(nullableStringSerializer);
			byte[] stringsBuffer = new byte[arraySerializer.GetSize(strings)];
			arraySerializer.Serialize(strings, stringsBuffer);
			string[] stringsResult = arraySerializer.Deserialize(stringsBuffer);

			int? nullableInt = 10;
			ConstantLengthNullableSerializer<int> constantLengthNullableSerializer = new(Int32Serializer.Default);
			byte[] buffer3 = new byte[constantLengthNullableSerializer.Size];
			constantLengthNullableSerializer.Serialize(nullableInt, buffer3);
			int? nullableIntResult = constantLengthNullableSerializer.Deserialize(buffer3);

			Guid guid = Guid.NewGuid();
			GuidSerializer guidSerializer = new GuidSerializer();
			byte[] guidBuffer = new byte[guidSerializer.Size];
			guidSerializer.Serialize(guid, guidBuffer);
			Guid guidResult = guidSerializer.Deserialize(guidBuffer);
		}
	}
}
