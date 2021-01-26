using Devdeb.Sorage.SorableHeap;
using System.IO;
using System.Text;

namespace Devdeb.Storage.Test.StorableHeapTests
{
	internal class StorableHeapTest
	{
		public const string DatabaseDirectory = @"C:\Users\lehac\Desktop\data";
		public const long MaxHeapSize = 10000;

		public DirectoryInfo DatabaseDirectoryInfo => new DirectoryInfo(DatabaseDirectory);

		public void Test()
		{
			StorableHeap storableHeap = new StorableHeap(DatabaseDirectoryInfo, MaxHeapSize);
			Segment segment = storableHeap.AllocateMemory(10);
			Segment segment1 = storableHeap.AllocateMemory(10);
			Segment segment2 = storableHeap.AllocateMemory(10);

			string testString = "TestStringulya";
			byte[] testStringBuffer = Encoding.UTF8.GetBytes(testString);
			Segment stringSegment = storableHeap.AllocateMemory(testStringBuffer.Length);
			storableHeap.Write(stringSegment, testStringBuffer, 0, testStringBuffer.Length);

			stringSegment = new Segment
			{
				Pointer = 30,
				Size = 14
			};
			byte[] readBuffer = new byte[stringSegment.Size];
			storableHeap.ReadBytes(stringSegment, readBuffer, 0, readBuffer.Length);
			string readString = Encoding.UTF8.GetString(readBuffer);

			storableHeap.FreeMemory(segment1);
		}
	}
}
