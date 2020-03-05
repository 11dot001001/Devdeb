using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Devdeb.Tests.Common
{
	[TestClass]
	public class CommonTest
	{

		[TestMethod]
		public void RunTest()
		{
			//int[] indexes = new int[]
			//{
			//	58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4, 62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
			//	57, 49, 41, 33, 25, 17, 9,  1, 59, 51, 43, 35, 27, 19, 11, 3, 61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7
			//};
			int[] indexes = new int[]
			{
				9, 10, 11, 12, 13, 14, 15, 16, 17, 52, 44, 36, 28, 20, 12, 4, 62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
				57, 49, 41, 33, 25, 17, 9,  1, 59, 51, 43, 35, 27, 19, 11, 3, 61, 53, 45, 37, 29, 21, 13, 5, 9, 10, 11, 12, 13, 14, 15, 16
			};
			for (int i = 0; i < indexes.Length; i++)
				indexes[i] -= 1;

			byte[] a = new byte[] { 2, 211, 0, 1, 0, 0, 0, 0 };

			byte[] result = Permutator.PermuteBits(a, indexes);

			Console.ReadKey();
		}

		public void PermuteInitialData(byte[] bytes)
		{

		}
	}

	public static class Permutator
	{
		private const int BitsInByteCount = 8;

		static public unsafe byte[] PermuteBits(byte[] bytes, int[] permutationIndexesMap)
		{
			if (bytes.Length * BitsInByteCount != permutationIndexesMap.Length)
				throw new Exception($"The length of bits of {nameof(bytes)} does not match to length of {nameof(permutationIndexesMap)}.");

			byte[] permutationBuffer = new byte[bytes.Length];
			int permutationBufferOffset = 0;
			for (int permutationMapOffset = 0; permutationMapOffset < permutationIndexesMap.Length; permutationMapOffset++)
			{
				int targetPermutationIndex = permutationIndexesMap[permutationMapOffset];
				int byteOffset = targetPermutationIndex % BitsInByteCount;
				byte targetPermutationBit = (byte)((bytes[targetPermutationIndex / BitsInByteCount] >> byteOffset) & 1);

				byteOffset = permutationBufferOffset % BitsInByteCount;
				permutationBuffer[permutationBufferOffset++ / BitsInByteCount] |= (byte)(targetPermutationBit << byteOffset);
			}
			return permutationBuffer;
		}
	}
}