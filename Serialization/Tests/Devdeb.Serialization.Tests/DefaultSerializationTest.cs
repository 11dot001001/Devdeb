using Devdeb.Serialization.Builders;
using Devdeb.Serialization.Default;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using System.Collections.Generic;

namespace Devdeb.Serialization.Tests
{
	public class DefaultSerializationTest
	{
		public void Test()
		{
			ISerializer<TestClass[]> serializer1 = DefaultSerializer<TestClass[]>.Instance;
			ISerializer<int?[]> serializer2 = DefaultSerializer<int?[]>.Instance;
			ISerializer<IEnumerable<int>> serializer3 = DefaultSerializer<IEnumerable<int>>.Instance;
			ISerializer<int[]> serializer4 = DefaultSerializer<int[]>.Instance;
			ISerializer<TestClass> serializer = DefaultSerializer<TestClass>.Instance;

			TestClass testClass = new TestClass
			{
				IntValue = 15,
				StringValue = "Some string",
				Gender = Gender.Female
			};

			byte[] buffer = new byte[serializer.GetSize(testClass)];
			serializer.Serialize(testClass, buffer);
			TestClass result = serializer.Deserialize(buffer);

			Test2();
		}
		public void Test2()
		{
			TestClass testClass = new TestClass
			{
				IntValue = 15,
				StringValue = "Some string",
				IntValue2 = 13213
			};

			SerializerBuilder<TestClass> serializerBuilder = new SerializerBuilder<TestClass>();
			serializerBuilder.AddMember(x => x.IntValue, Int32Serializer.Default);
			serializerBuilder.AddMember(x => x.StringValue, StringLengthSerializer.Default);
			ISerializer<TestClass> serializer = serializerBuilder.Build();

			byte[] buffer = new byte[serializer.GetSize(testClass)];
			serializer.Serialize(testClass, buffer);
			TestClass result = serializer.Deserialize(buffer);
		}

		public class TestClass
		{
			public int IntValue2;
			public int IntValue { get; set; }
			public string StringValue { get; set; }
			public Gender Gender { get; set; }

			public void AddTest() { }
		}
		public enum Gender : byte
		{
			Male = 1,
			Female = 2
		}
	}
}
