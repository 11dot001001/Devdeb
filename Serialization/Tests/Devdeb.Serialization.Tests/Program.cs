using System;

namespace Devdeb.Serialization.Tests
{
	class Program
	{
		private static readonly SerializationTest _serializationTest;

		static unsafe Program()
		{
			_serializationTest = new SerializationTest();
		}

		static private void Main(string[] args) => _serializationTest.Test();
	}
}
