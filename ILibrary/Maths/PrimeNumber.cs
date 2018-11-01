using System.Collections.Generic;

namespace ILibrary
{
    namespace Maths
    {
        public static class PrimeNumber
        {
            /// <summary>
            /// Returns a list of Prime numbers
            /// </summary>
            /// <param name="maxValue">To what values to return the number</param>
            /// <returns></returns>
            public static IEnumerable<int> GetNumbers(int maxValue)
            {
                List<int> listSimpleNumber = new List<int>();
                int[] a = new int[maxValue];
                for (int j = 0; j < maxValue; j++)
                    a[j] = j;

                a[1] = 0;
                int i = 2;
                while (i < maxValue)
                {
                    if (a[i] != 0)
                        listSimpleNumber.Add(a[i]);
                    for (int j = i; j <= a.Length + 1;)
                    {
                        if ((j += i) >= a.Length) break;
                        a[j] = 0;
                    }
                    i++;
                }
                return listSimpleNumber;
            }
        }
    }
}
