namespace Devdeb.Serialization.Tests
{
	class Program
	{
		private static readonly SerializationTest _serializationTest;
		private static readonly DefaultSerializationTest _defaultSerializationTest;

		static unsafe Program()
		{
			_serializationTest = new SerializationTest();
			_defaultSerializationTest = new DefaultSerializationTest();
		}

		static private void Main(string[] args) => _defaultSerializationTest.Test();
	}
}
