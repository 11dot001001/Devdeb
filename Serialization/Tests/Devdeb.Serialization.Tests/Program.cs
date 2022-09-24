using System;

namespace Devdeb.Serialization.Tests
{
	class Program
	{
		private static readonly SerializationTest _serializationTest;
		private static readonly DefaultSerializationTest _defaultSerializationTest;
		private static readonly ObjectSerializationTest _objectSerializationTest;
		
		static unsafe Program()
		{
			_serializationTest = new SerializationTest();
			_defaultSerializationTest = new DefaultSerializationTest();
			_objectSerializationTest = new ObjectSerializationTest();
		}

		static private void Main(string[] args) => _defaultSerializationTest.Test();
	}
}
