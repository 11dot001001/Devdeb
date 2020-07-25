using Devdeb.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devdeb.Tests.Common
{
	class Program
	{
		static void Main(string[] args)
		{
			List<int> primeNumbers = PrimeNumber.GetNumbers(byte.MaxValue).ToList();
			checked
			{
				int p = 181;
				int q = 251;
				int n = p * q;
				int eulerFunction = (p - 1) * (q - 1);
				int e = 13;
				int d = FindD(e, eulerFunction);

				int a = byte.MaxValue;
				int c = ModularPow(a, e, n);
				int aa = ModularPow(c, d, n);
			 
			}
		}
		static private int ModularPow(int bases, int index, int modulus)
		{
			checked
			{
				int c = 1;
				for (int i = 1; i != index + 1; i++)
					c = (c * bases) % modulus;
				return c;
			}
		}

		private static int FindD(long e, long eulerFunction)
		{
			checked
			{
				for (int i = 1; ; i++)
				{
					long de = eulerFunction * i + 1;
					if (de % e != 0)
						continue;
					long d = de / e;
					if (d == e)
						continue;
					return (int)d;
				}
			}
		}
	}

	public class RSA
	{

	}
}