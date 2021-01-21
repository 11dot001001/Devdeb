using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Text;

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
			NullableSerializer<string> nullableStringSerializer = new NullableSerializer<string>(new StringLengthSerializer(Encoding.Default));
			ArrayLengthSerializer<string> arraySerializer = new ArrayLengthSerializer<string>(nullableStringSerializer);
			byte[] stringsBuffer = new byte[arraySerializer.Size(strings)];
			arraySerializer.Serialize(strings, stringsBuffer, 0);
			string[] stringsResult = arraySerializer.Deserialize(stringsBuffer, 0);

			int? nullableInt = 10;
			ConstantLengthNullableSerializer<int> constantLengthNullableSerializer = new ConstantLengthNullableSerializer<int>(new Int32Serializer());
			byte[] buffer3 = new byte[constantLengthNullableSerializer.Size];
			constantLengthNullableSerializer.Serialize(nullableInt, buffer3, 0);
			int? nullableIntResult = constantLengthNullableSerializer.Deserialize(buffer3, 0);

			Guid guid = Guid.NewGuid();
			GuidSerializer guidSerializer = new GuidSerializer();
			byte[] guidBuffer = new byte[guidSerializer.Size];
			guidSerializer.Serialize(guid, guidBuffer, 0);
			Guid guidResult = guidSerializer.Deserialize(guidBuffer, 0);
		}
	}
}
