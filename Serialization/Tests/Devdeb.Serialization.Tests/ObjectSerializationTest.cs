using Devdeb.Serialization.Serializers.Objects;

namespace Devdeb.Serialization.Tests
{
	public class ObjectSerializationTest
	{
		public class Student
		{
			public int Age { get; set; }
			public string Name { get; set; }
		}

		public void Test()
		{
			ObjectArraySerializer objectArraySerializer = new ObjectArraySerializer(
				new ObjectSerializer(typeof(Student)),
				new ObjectSerializer(typeof(int)),
				new ObjectSerializer(typeof(Student))
			);
			object[] send = new object[3]
			{
				new Student
				{
					Age = 121212,
					Name = "dsfgksdfgoj nhspgkdfg dfsggs"
				},
				null,
				new Student
				{
					Age = 23353453,
					Name = "Student 2"
				}
			};

			byte[] buffer = new byte[objectArraySerializer.GetSize(send)];
			objectArraySerializer.Serialize(send, buffer);
			var result = objectArraySerializer.Deserialize(buffer);
		}
	}
}
