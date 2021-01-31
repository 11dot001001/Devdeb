using Devdeb.Serialization.Builders;
using Devdeb.Serialization.Default;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;

namespace Devdeb.Serialization.Tests
{
	public class DefaultSerializationTest
	{
		public void Test()
		{
			ISerializer<TestClass> serializer = DefaultSerializer<TestClass>.Instance;
			
			TestClass testClass = new TestClass
			{
				IntValue = 15,
				StringValue = "Some string"
			};

			byte[] buffer = new byte[serializer.Size(testClass)];
			serializer.Serialize(testClass, buffer, 0);
			TestClass result = serializer.Deserialize(buffer, 0);

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

			byte[] buffer = new byte[serializer.Size(testClass)];
			serializer.Serialize(testClass, buffer, 0);
			TestClass result = serializer.Deserialize(buffer, 0);
		}

		public class TestClass
		{
			public int IntValue2;
			public int IntValue { get; set; }
			public string StringValue { get; set; }

			public void AddTest() { }
		}
	}
}
