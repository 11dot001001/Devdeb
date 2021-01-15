using Devdeb.Sets.Generic;
using Devdeb.Sets.Ratios;
using System;
using System.Diagnostics;

namespace Devdeb.Storage.Test.RedBlackTreeSurjectionTests
{
	internal class RedBlackTreeSurjectionTest
	{
		private class Class
		{
			static private Random _random = new Random();

			public Guid Id { get; set; } = Guid.NewGuid();
			public int IntValue { get; } = _random.Next(int.MinValue, int.MaxValue);
		}

		public void Test()
		{
			RedBlackTreeSurjection<Guid, Class> surjection = new RedBlackTreeSurjection<Guid, Class>();
			for (int i = 0; i < 10000000; i++)
			{
				Class temp = new Class();
				surjection.TryAdd(temp.Id, temp);
				if (i % 3 == 0)
					Debug.Assert(surjection.Remove(temp.Id));
			}
			foreach (SurjectionRatio<Guid, Class> ratio in surjection)
				Console.WriteLine($"{ratio.Input} - {ratio.Output.IntValue}");
		}
	}
}
