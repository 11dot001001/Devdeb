using Devdeb.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Devdeb.Tests.Cryptography
{
	[TestClass]
	public class DataEncryptionStandardTest
	{
		private readonly string _key = "Password";

		[TestMethod]
		public void ExecuteCommonTest()
		{
			Random random = new Random();
			DataEncryptionStandard dataEncryptionStandard = new DataEncryptionStandard(_key);
			for (int i = 0; i < 10000000; i++)
			{
				byte[] buffer = new byte[random.Next(1, 4096)];
				random.NextBytes(buffer);
				byte[] bytes = dataEncryptionStandard.Encrypt(buffer);
				bytes = dataEncryptionStandard.Decrypt(bytes);
				for (int j = 0; j < buffer.Length; j++)
					Assert.AreEqual(buffer[j], bytes[j]);
			}
		}
	}
}