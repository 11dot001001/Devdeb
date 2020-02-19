using System.Collections.Generic;

namespace Devdeb
{
    namespace Maths
    {
        public static class PrimeNumber
        {
            /// <summary>
            /// Returns a list of prime numbers
            /// </summary>
            /// <param name="maxValue">To what values to return the number</param>
            /// <returns></returns>
            public static IEnumerable<int> GetNumbers(int maxValue)
            {
                List<int> listSimpleNumber = new List<int>();
                int[] array = new int[maxValue];
                for (int j = 0; j < maxValue; j++)
                    array[j] = j;

                array[1] = 0;
                int i = 2;
                while (i < maxValue)
                {
                    if (array[i] != 0)
                        listSimpleNumber.Add(array[i]);
                    for (int j = i; j <= array.Length + 1;)
                    {
                        if ((j += i) >= array.Length) 
							break;
                        array[j] = 0;
                    }
                    i++;
                }
                return listSimpleNumber;
            }
        }
    }
}