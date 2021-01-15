using System;

namespace Devdeb.Sets.Extensions
{
	public static class ArrayExtensions
	{
		public const int MaxArrayLenghtFor8BitElement = 0X7FFFFFC7;
		public const int MaxArrayLenghtFor9To128BitElement = 0X7FEFFFFF;

		public static void EnsureLength<T>(ref T[] array, int desiredLength)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (desiredLength < 0)
				throw new ArgumentOutOfRangeException(nameof(desiredLength));

			if (array.Length >= desiredLength)
				return;

			T[] newArray = new T[desiredLength];
			Array.Copy(array, 0, newArray, 0, array.Length);
			array = newArray;
		}
		public static void IncreaseLength<T>(ref T[] array)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			int increasedLength = array.Length << 1;
			if (increasedLength < array.Length)
				increasedLength = int.MaxValue;

			EnsureLength(ref array, increasedLength);
		}
	}
}
