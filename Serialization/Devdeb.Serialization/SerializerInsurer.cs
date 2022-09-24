using System;

namespace Devdeb.Serialization
{
	internal static class SerializerInsurer
	{
		public static void VerifyDeserialize(ReadOnlySpan<byte> buffer, int instanceSize)
		{
			if (buffer.Length < instanceSize)
				throw new Exception($"The {nameof(instanceSize)} exceeds {nameof(buffer.Length)}.");
		}
	}
}
