using Devdeb.Storage.Test.DataSourceTests;
using Devdeb.Storage.Test.FileStreamApiTests;
using Devdeb.Storage.Test.RedBlackTreeSurjectionTests;
using Devdeb.Storage.Test.StorableHeapTests;

namespace Devdeb.Storage.Test
{
	class Program
	{
		private static readonly StorableHeapTest _storableHeapTest;
		private static readonly RedBlackTreeSurjectionTest _redBlackSurjectionTest;
		private static readonly DataSourceTest _dataSourceTest;
		private static readonly FileStreamApiTest _fileStreamApiTest;

		static Program()
		{
			_storableHeapTest = new StorableHeapTest();
			_redBlackSurjectionTest = new RedBlackTreeSurjectionTest();
			_dataSourceTest = new DataSourceTest();
			_fileStreamApiTest = new FileStreamApiTest();
		}

		static private void Main(string[] args) => _dataSourceTest.Test();
	}
}
