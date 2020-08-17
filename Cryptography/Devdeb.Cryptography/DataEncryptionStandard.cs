using System;
using System.Text;

namespace Devdeb.Cryptography
{
	public class DataEncryptionStandard
	{
		private enum ActionMode { Encrypt, Decrypt }

		private const int BitsInByteCount = 8;
		private const int FeistelFunctionDataBitsCount = 32;
		private const int FeistelFunctionKeyBitsCount = 48;
		private const int ProcessingCycleDataBitsCount = 64;
		private const int ProcessingCycleDataBytesCount = 8;
		private const int KeyASCILength = 8;
		private const int FieldSizeOfCountExtendedBytes = 1;

		private readonly byte[][] _keys = new byte[16][];


		public unsafe DataEncryptionStandard(string keyString)
		{
			if (string.IsNullOrEmpty(keyString))
				throw new Exception($"{nameof(keyString)} is null or empty.");
			if (keyString.Length != KeyASCILength)
				throw new Exception($"The length of {nameof(keyString)} doesn't valid. Expected {KeyASCILength}.");

			byte[] keyBytes = Encoding.ASCII.GetBytes(keyString);
			int[] indexesMap = new int[56]
			{
				57, 49, 41, 33, 25, 17, 9,  1,  58, 50, 42, 34, 26, 18,
				10, 2,  59, 51, 43, 35, 27, 19, 11, 3,  60, 52, 44, 36,
				63, 55, 47, 39, 31, 23, 15, 7,  62, 54, 46, 38, 30, 22,
				14, 6,  61, 53, 45, 37, 29, 21, 13, 5,  28, 20, 12, 4,
			};
			for (int i = 0; i != indexesMap.Length; i++)
				indexesMap[i] -= 1;
			byte[] key = Permutator.PermuteBits(keyBytes, indexesMap);
			byte[] temp = new byte[key.Length + 1];
			for (int i = 0; i < key.Length; i++)
				temp[i] = key[i];
			key = temp;

			int[] cArrayIndexes = new int[28]
			{
				57, 49, 41, 33, 25, 17, 9 , 1 , 58, 50, 42, 34, 26, 18,
				10, 2 , 59, 51, 43, 35, 27, 19, 11, 3 , 60, 52, 44, 36,
			};
			for (int i = 0; i != cArrayIndexes.Length; i++)
				cArrayIndexes[i] -= 1;
			int[] dArrayIndexes = new int[28]
			{
				63, 55, 47, 39, 31, 23, 15, 7 , 62, 54, 46, 38, 30, 22,
				14, 6 , 61, 53, 45, 37, 29, 21, 13, 5 , 28, 20, 12, 4 ,
			};
			for (int i = 0; i != dArrayIndexes.Length; i++)
				dArrayIndexes[i] -= 1;
			byte[] cArray = Permutator.PermuteBits(key, cArrayIndexes);
			byte[] dArray = Permutator.PermuteBits(key, dArrayIndexes);

			uint c = 0;
			uint d = 0;
			fixed (byte* cArrayPointer = &cArray[0])
				c = *(uint*)cArrayPointer;
			fixed (byte* dArrayPointer = &dArray[0])
				d = *(uint*)dArrayPointer;
			c <<= 4;
			d <<= 4;

			int[] keyIndexesMap = new int[48]
			{
				14, 17, 11, 24, 1 , 5 , 3 , 28, 15, 6 , 21, 10,
				23, 19, 12, 4 , 26, 8 , 16, 7 , 27, 20, 13, 2 ,
				41, 52, 31, 37, 47, 55, 30, 40, 51, 45, 33, 48,
				44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32,
			};
			for (int i = 0; i != _keys.Length; i++)
			{
				int shiftCount = 2;
				if (i == 0 || i == 1 || i == 8 || i == 15)
					shiftCount = 1;
				c = c << shiftCount | ((c >> (28 - shiftCount)) & (uint)(shiftCount == 1 ? 0b0001_0000 : 0b0011_0000));
				d = d << shiftCount | ((d >> (28 - shiftCount)) & (uint)(shiftCount == 1 ? 0b0001_0000 : 0b0011_0000));
				ulong combinationCD = 0;
				for (int j = 0; j != 28; j++)
				{
					combinationCD |= ((c >> (31 - j)) & 1) << (63 - j);
					combinationCD |= ((d >> (31 - j)) & 1) << (63 - 28 - j);
				}
				byte[] combinationCDBytes = new byte[8];
				fixed (byte* pointer = &combinationCDBytes[0])
					*(ulong*)pointer = combinationCD;
				_keys[i] = Permutator.PermuteBits(combinationCDBytes, keyIndexesMap);
			}
		}

		private byte[] ExecuteFeistelFunction(byte[] bytes, byte[] key)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (bytes.Length * BitsInByteCount != FeistelFunctionDataBitsCount)
				throw new Exception($"The bits length of {nameof(bytes)} doesn't vaild. Expected {FeistelFunctionDataBitsCount}.");
			if (key.Length * BitsInByteCount != FeistelFunctionKeyBitsCount)
				throw new Exception($"The bits length of {nameof(key)} doesn't vaild. Expected {FeistelFunctionKeyBitsCount}.");

			int[] extendedIndexesMap = new int[]
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
			for (int i = 0; i != extendedIndexesMap.Length; i++)
				extendedIndexesMap[i] -= 1;
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

			byte[] extendedBytes = Permutator.PermuteBits(bytes, extendedIndexesMap);

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
		private unsafe byte[] ExecuteProcessingCycle(byte[] bytes, ActionMode actionMode)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (bytes.Length * BitsInByteCount != ProcessingCycleDataBitsCount)
				throw new Exception($"The bits length of {bytes} doesn't vaild. Expected {ProcessingCycleDataBitsCount}.");

			int[] indexesMap = new int[64]
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
				byte[] tempBytes = ExecuteFeistelFunction(rightPiece, _keys[actionMode == ActionMode.Encrypt ? i : 15 - i]);
				for (int j = 0; j < tempBytes.Length; j++)
					tempBytes[j] ^= leftPiece[j];
				leftPiece = rightPiece;
				rightPiece = tempBytes;
			}
			byte[] result = new byte[8];
			fixed (byte* resultPointer = &result[0])
			{
				fixed (byte* leftPiecePointer = &leftPiece[0])
					*((int*)resultPointer + 1) = *(int*)leftPiecePointer;
				fixed (byte* rightPiecePointer = &rightPiece[0])
					*(int*)resultPointer = *(int*)rightPiecePointer;
			}
			int[] finalPermutationIndexes = new int[64]
			{
				40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
				38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
				36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
				34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9 , 49, 17, 57, 25,
			};
			for (int i = 0; i != finalPermutationIndexes.Length; i++)
				finalPermutationIndexes[i] -= 1;
			return Permutator.PermuteBits(result, finalPermutationIndexes);
		}

		public unsafe byte[] Encrypt(byte[] bytes)
		{
			int piecesOf64Bits = (bytes.Length + BitsInByteCount - 1) / BitsInByteCount;
			byte[] result = new byte[piecesOf64Bits * ProcessingCycleDataBytesCount + FieldSizeOfCountExtendedBytes];
			result[0] = (byte)(result.Length - FieldSizeOfCountExtendedBytes - bytes.Length);

			byte[] workingBuffer = new byte[ProcessingCycleDataBytesCount];
			for (int i = 0; i != piecesOf64Bits; i++)
			{
				fixed (byte* bytesIterationPointer = &bytes[ProcessingCycleDataBytesCount * i])
				fixed (byte* workingBufferPointer = &workingBuffer[0])
					*(ulong*)workingBufferPointer = *(ulong*)bytesIterationPointer;
				workingBuffer = ExecuteProcessingCycle(workingBuffer, ActionMode.Encrypt);
				fixed (byte* resultIterationPointer = &result[ProcessingCycleDataBytesCount * i + 1])
				fixed (byte* workingBufferPointer = &workingBuffer[0])
					*(ulong*)resultIterationPointer = *(ulong*)workingBufferPointer;
			}
			return result;
		}
		public unsafe byte[] Decrypt(byte[] bytes)
		{
			byte extendedBytesCount = bytes[0];
			int piecesOf64Bits = (bytes.Length - FieldSizeOfCountExtendedBytes + BitsInByteCount - 1) / BitsInByteCount;

			byte[] result = new byte[bytes.Length - extendedBytesCount - FieldSizeOfCountExtendedBytes];
			byte[] workingBuffer = new byte[ProcessingCycleDataBytesCount];
			for (int i = 0; i != piecesOf64Bits; i++)
			{
				fixed (byte* bytesIterationPointer = &bytes[ProcessingCycleDataBytesCount * i + 1])
				fixed (byte* workingBufferPointer = &workingBuffer[0])
					*(ulong*)workingBufferPointer = *(ulong*)bytesIterationPointer;

				workingBuffer = ExecuteProcessingCycle(workingBuffer, ActionMode.Decrypt);

				if (i == piecesOf64Bits - 1)
				{
					for (int j = 0; j != ProcessingCycleDataBytesCount - extendedBytesCount; j++)
						result[ProcessingCycleDataBytesCount * i + j] = workingBuffer[j];
					break;
				}

				fixed (byte* resultIterationPointer = &result[ProcessingCycleDataBytesCount * i])
				fixed (byte* workingBufferPointer = &workingBuffer[0])
					*(ulong*)resultIterationPointer = *(ulong*)workingBufferPointer;
			}

			return result;
		}
	}
}