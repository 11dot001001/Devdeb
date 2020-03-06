using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Devdeb.Tests.Common
{
	[TestClass]
	public class CommonTest
	{
		[TestMethod]
		public unsafe void RunTest()
		{
			byte[] sourceBytes = Encoding.ASCII.GetBytes("TestFalq");
			DataEncryptionStandard dataEncryptionStandard = new DataEncryptionStandard();
			byte[] bytes = dataEncryptionStandard.Encrypt(sourceBytes);
			Console.ReadKey();
		}
	}

	public class DataEncryptionStandard
	{
		private const int BitsInByteCount = 8;
		private const int FeistelFunctionDataBitsCount = 32;
		private const int FeistelFunctionKeyBitsCount = 48;
		private const int EncryptionCycleDataBitsCount = 64;

		private byte[] ExecuteFeistelFunction(byte[] bytes, byte[] key)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (bytes.Length * BitsInByteCount != FeistelFunctionDataBitsCount)
				throw new Exception($"The bits length of {bytes} doesn't vaild. Expected {FeistelFunctionDataBitsCount}.");
			if (key.Length * BitsInByteCount != FeistelFunctionKeyBitsCount)
				throw new Exception($"The bits length of {key} doesn't vaild. Expected {FeistelFunctionKeyBitsCount}.");

			int[] extendedIndexMap = new int[]
			{
				32, 1,  2 , 3 , 4,  5,
				4,  5,  6 , 7 , 8,  9,
				8,  9,  10, 11, 12, 13,
				12, 13, 14, 15, 16, 17,
				16, 17, 18, 19, 20, 21,
				20, 21, 22, 23, 24, 25,
				24, 25, 26, 27, 28, 29,
				28, 29, 30, 31, 32, 1
			};
			for (int i = 0; i != extendedIndexMap.Length; i++)
				extendedIndexMap[i] -= 1;
			byte[][][] sArray = new byte[][][]
			{
				new byte[][]
				{
					new byte[] { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 },
					new byte[] { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8 },
					new byte[] { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0 },
					new byte[] { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 }
				},
				new byte[][]
				{
					new byte[] { 15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10 },
					new byte[] { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5 },
					new byte[] { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15 },
					new byte[] { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9 }
				},
				new byte[][]
				{
					new byte[] { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 },
					new byte[] { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1 },
					new byte[] { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 },
					new byte[] { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12 }
				},
				new byte[][]
				{
					new byte[] { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15 },
					new byte[] { 13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9 },
					new byte[] { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4 },
					new byte[] { 3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14 }
				},
				new byte[][]
				{
					new byte[] { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9 },
					new byte[] { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6 },
					new byte[] { 4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14 },
					new byte[] { 11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3 }
				},
				new byte[][]
				{
					new byte[] { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 },
					new byte[] { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 },
					new byte[] { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 },
					new byte[] { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 }
				},
				new byte[][]
				{
					new byte[] { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1 },
					new byte[] { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6 },
					new byte[] { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2 },
					new byte[] { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12 }
				},
				new byte[][]
				{
					new byte[] { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7 },
					new byte[] { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2 },
					new byte[] { 7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8 },
					new byte[] { 2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11 }
				},
			};

			byte[] extendedBytes = Permutator.PermuteBits(bytes, extendedIndexMap);

			for (int i = 0; i < extendedBytes.Length; i++)
				extendedBytes[i] ^= key[i];

			byte[] sBytes = new byte[4];
			int extendedBytesOffset = 0;
			for (int i = 0; i < 8; i++)
			{
				byte bits6B2 = 0;
				byte bits6B4 = 0;
				bits6B2 |= (byte)((extendedBytes[extendedBytesOffset / 8] >> (extendedBytesOffset++ % 8)) & 1);
				bits6B4 |= (byte)((extendedBytes[extendedBytesOffset / 8] >> (extendedBytesOffset++ % 8)) & 1);
				bits6B4 |= (byte)(((extendedBytes[extendedBytesOffset / 8] >> (extendedBytesOffset++ % 8)) & 1) << 1);
				bits6B4 |= (byte)(((extendedBytes[extendedBytesOffset / 8] >> (extendedBytesOffset++ % 8)) & 1) << 2);
				bits6B4 |= (byte)(((extendedBytes[extendedBytesOffset / 8] >> (extendedBytesOffset++ % 8)) & 1) << 3);
				bits6B2 |= (byte)(((extendedBytes[extendedBytesOffset / 8] >> (extendedBytesOffset++ % 8)) & 1) << 1);

				byte s = sArray[i][bits6B2][bits6B4];
				sBytes[i / 2] |= (byte)(s << (i % 2 * 4));
			}

			int[] resultIndexMap = new int[]
			{
				16, 7,  20, 21, 29, 12, 28, 17,
				1,  15, 23, 26, 5,  18, 31, 10,
				2,  8,  24, 14, 32, 27, 3,  9,
				19, 13, 30, 6,  22, 11, 4,  25
			};
			for (int i = 0; i != resultIndexMap.Length; i++)
				resultIndexMap[i] -= 1;

			return Permutator.PermuteBits(sBytes, resultIndexMap);
		}

		private unsafe byte[] ExecuteEncryptionCycle(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (bytes.Length * BitsInByteCount != EncryptionCycleDataBitsCount)
				throw new Exception($"The bits length of {bytes} doesn't vaild. Expected {EncryptionCycleDataBitsCount}.");

			int[] indexesMap = new int[]
			{
				58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4, 62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
				57, 49, 41, 33, 25, 17, 9,  1, 59, 51, 43, 35, 27, 19, 11, 3, 61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7
			};
			for (int i = 0; i != indexesMap.Length; i++)
				indexesMap[i] -= 1;

			byte[] initialPermutation = Permutator.PermuteBits(bytes, indexesMap);

			byte[] leftPiece = new byte[4];
			byte[] rightPiece = new byte[4];
			fixed (byte* initialPermutationPointer = &initialPermutation[0])
			{
				fixed (byte* leftPiecePointer = &leftPiece[0])
					*(int*)leftPiecePointer = *(int*)initialPermutationPointer;
				fixed (byte* rightPiecePointer = &rightPiece[0])
					*(int*)rightPiecePointer = *((int*)initialPermutationPointer + 1);
			}

			for (int i = 0; i != 16; i++)
			{

			}

			return null;
		}

		public byte[] Encrypt(byte[] bytes)
		{
			return null;
		}

		//public byte[] Decrypt(byte[] bytes)
		//{

		//}
	}

	public static class Permutator
	{
		private const int BitsInByteCount = 8;

		public static byte[] PermuteBits(byte[] bytes, int[] permutationIndexesMap)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (permutationIndexesMap == null)
				throw new ArgumentNullException(nameof(permutationIndexesMap));

			byte[] permutationBuffer = new byte[permutationIndexesMap.Length / BitsInByteCount];
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
		public static byte[] InversePermuteBits(byte[] bytes, int[] permutationIndexesMap)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (permutationIndexesMap == null)
				throw new ArgumentNullException(nameof(permutationIndexesMap));

			byte[] permutationBuffer = new byte[permutationIndexesMap.Length / BitsInByteCount];
			for (int permutationBufferOffset = 0; permutationBufferOffset < permutationIndexesMap.Length; permutationBufferOffset++)
			{
				int byteOffset = permutationBufferOffset % BitsInByteCount;
				byte targetPermutationBit = (byte)((bytes[permutationBufferOffset / BitsInByteCount] >> byteOffset) & 1);

				int targetPermutationIndex = permutationIndexesMap[permutationBufferOffset];
				byteOffset = targetPermutationIndex % BitsInByteCount;
				permutationBuffer[targetPermutationIndex / BitsInByteCount] |= (byte)(targetPermutationBit << byteOffset);
			}
			return permutationBuffer;
		}
	}
}