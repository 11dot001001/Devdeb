using System;

namespace Devdeb.Cryptography
{
    public static class Permutator
	{
		private const int BitsInByteCount = 8;

		public static byte[] PermuteBits(byte[] bytes, int[] permutationIndexesMap)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (permutationIndexesMap == null)
				throw new ArgumentNullException(nameof(permutationIndexesMap));

			byte[] permutationBuffer = new byte[(permutationIndexesMap.Length + BitsInByteCount - 1) / BitsInByteCount];
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

			byte[] permutationBuffer = new byte[(permutationIndexesMap.Length + BitsInByteCount - 1) / BitsInByteCount];
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