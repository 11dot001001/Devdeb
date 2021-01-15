using System.IO;

namespace Devdeb.Storage.Test.FileStreamApiTests
{
	internal class FileStreamApiTest
	{
		public const string DatabaseDirectory = @"C:\Users\lehac\Desktop\data";

		public void Test()
		{
			string filePath = Path.Combine(DatabaseDirectory, "_data");
			using FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
			byte[] buffer = new byte[5000000];
			fileStream.Write(buffer, 0, 4000);
			buffer[0] = 1;
			fileStream.Flush(true);
			_ = fileStream.Seek(0, SeekOrigin.Begin);
			int readBytes = fileStream.Read(buffer, 0, 3);
		}
	}
}
