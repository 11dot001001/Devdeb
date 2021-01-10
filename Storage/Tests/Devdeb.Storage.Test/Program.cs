using Devdeb.Sets.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Devdeb.Storage.Test
{
	class Program
	{
		public const string DatabaseDirectory = @"C:\Users\lehac\Desktop\data";
		public const long MaxHeapSize = 10000;

		public const int ByteSize = 0X7FFFFFC7;
		public const int MoreByteSize = 0X7FEFFFFF;

		static void Main(string[] args)
		{
			TestStorableHeap();
			Dictionary<int, ClassB> keys = new Dictionary<int, ClassB>();
			SortedDictionary<int, ClassB> sortedDick = new SortedDictionary<int, ClassB>();
			//sortedDick.TryGetValue
			//Test2();
			Test1();
		}
		static void Test2()
		{
			RedBlackTreeSurjection<int, ClassB> surjection = new RedBlackTreeSurjection<int, ClassB>(Comparer<int>.Default);
			ClassB b1 = new ClassB { Id = 2 };
			ClassB b2 = new ClassB { Id = 3 };
			surjection.TryAdd(b1.Id, b1);
			surjection.TryAdd(b2.Id, b2);
			var res = surjection.Remove(b2.Id);
		}

		static void Test1()
		{
			RedBlackTreeSurjection<Guid, Class> surjection = new RedBlackTreeSurjection<Guid, Class>(Comparer<Guid>.Default);
			for (int i = 0; i < 10000000; i++)
			{
				Class temp = new Class();
				surjection.TryAdd(temp.Id, temp);
				if (i%3 ==0)
					Debug.Assert(surjection.Remove(temp.Id));
			}
		}

		static void TestBinaryTree()
		{
			BinaryTree<Class> binaryTree = new BinaryTree<Class>(1, new ClassComparator());

			for (int i = 0; i < 1000; i++)
			{
				binaryTree.Add(new Class());
			}
			Class searchClass = new Class();
			binaryTree.Add(searchClass);
			for (int i = 0; i < 1000; i++)
			{
				binaryTree.Add(new Class());
			}
			Class result = binaryTree.Search(new Class { Id = searchClass.Id });
		}


		static void TestStorableHeap()
		{
			StorableHeap storableHeap = new StorableHeap(new DirectoryInfo(DatabaseDirectory), MaxHeapSize);
			Segment segment = storableHeap.AllocateMemory(10);
			Segment segment1 = storableHeap.AllocateMemory(10);
			Segment segment2 = storableHeap.AllocateMemory(10);
			Segment segment3 = storableHeap.AllocateMemory(10);
			Segment segment4 = storableHeap.AllocateMemory(10);

			storableHeap.FreeMemory(segment1);
			Segment segment5 = storableHeap.AllocateMemory(5000);
			storableHeap.Write(segment5, new byte[] { 1 }, 0, 1);

			storableHeap.FreeMemory(segment);
			storableHeap.FreeMemory(segment2);
			storableHeap.FreeMemory(segment3);
			storableHeap.FreeMemory(segment4);
			storableHeap.Defragment();
			storableHeap.FreeMemory(new Segment { Pointer = 0, Size = 5000 });
			storableHeap.Defragment();
		}

		static void Test()
		{
			string filePath = Path.Combine(DatabaseDirectory, "_data");
			using FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
			byte[] buffer = new byte[5000000];
			fileStream.Write(buffer, 0, 4000);
			buffer[0] = 1;
			fileStream.Flush(true);
			fileStream.Seek(0, SeekOrigin.Begin);
			int readBytes = fileStream.Read(buffer, 0, 3);
		}
	}

	public class ClassB
	{
		static private Random _random = new Random();

		public int Id { get; set; }
		public int IntValue { get; } = _random.Next(int.MinValue, int.MaxValue);
	}
	public class Class
	{
		static private Random _random = new Random();

		public Guid Id { get; set; } = Guid.NewGuid();
		public int IntValue { get; } = _random.Next(int.MinValue, int.MaxValue);
	}
	public class ClassComparator : IBinaryTreeComparator<Class>
	{
		public Class NullValue => null;

		public ComparisonResult Compare(Class value1, Class value2)
		{
			if (IsNull(value1))
				throw new ArgumentNullException(nameof(value1));
			if (IsNull(value2))
				throw new ArgumentNullException(nameof(value2));

			int comparisonResult = value1.Id.CompareTo(value2.Id);
			if (comparisonResult == 0)
				return ComparisonResult.Equal;
			else if (comparisonResult > 0)
				return ComparisonResult.More;
			else if (comparisonResult < 0)
				return ComparisonResult.Less;
			throw new Exception();
		}

		public bool IsNull(Class value) => value == NullValue;
	}


	public class IntComparator : IBinaryTreeComparator<int>
	{
		private readonly int _nullValue;

		public IntComparator(int nullValue) => _nullValue = nullValue;

		public int NullValue => _nullValue;

		public ComparisonResult Compare(int value1, int value2)
		{
			int comparisonResult = value1.CompareTo(value2);
			if (comparisonResult == 0)
				return ComparisonResult.Equal;
			else if (comparisonResult > 0)
				return ComparisonResult.More;
			else if (comparisonResult < 0)
				return ComparisonResult.Less;
			throw new Exception();
		}

		public bool IsNull(int value) => _nullValue == value;
	}


	public enum ComparisonResult
	{
		Equal = 1,
		More = 2,
		Less = 3,
	}
	public interface IBinaryTreeComparator<T>
	{
		ComparisonResult Compare(T value1, T value2);
		bool IsNull(T value);
		T NullValue { get; }
	}
	public class BinaryTree<T>
	{
		public const int MaxLevel = 30;
		public const int MinLevel = 1;

		private T[] _elements;
		private readonly IBinaryTreeComparator<T> _comparator;
		private readonly Random _random;

		public BinaryTree(int levels, IBinaryTreeComparator<T> comparator)
		{
			if (levels < MinLevel || levels > MaxLevel)
				throw new ArgumentOutOfRangeException(nameof(levels));
			if (comparator == null)
				throw new ArgumentNullException(nameof(comparator));

			_elements = new T[1 << levels];
			_comparator = comparator;
			_random = new Random();
			FillNullValues(_elements, 0, _elements.Length);
		}

		public int Level
		{
			get
			{
				int level = 0;
				for (; (1 << level) != _elements.Length; level++) { }
				return level;
			}
		}
		public double LogLevel
		{
			get
			{
				return Math.Round(Math.Log(_elements.Length - 1, 2));
			}
		}
		private int NullElementsCount
		{
			get
			{
				int count = 0;
				for (int i = 0; i < _elements.Length; i++)
				{
					if (_comparator.IsNull(_elements[i]))
						count++;
				}
				return count;
			}
		}
		private int FilledElementsCount => _elements.Length - 1 - NullElementsCount;

		public void Add(T element)
		{
			if (_comparator.IsNull(element))
				throw new ArgumentNullException(nameof(element));

			int index = 0;
			for (; !_comparator.IsNull(_elements[index]);)
			{
				ComparisonResult direction = _comparator.Compare(element, _elements[index]);

				if (direction == ComparisonResult.Equal)
					direction = _random.Next(0, 2) == 1 ? ComparisonResult.Less : ComparisonResult.More;

				if (direction == ComparisonResult.Less)
					index = (index << 1) + 1;
				else if (direction == ComparisonResult.More)
					index = (index << 1) + 2;

				if (index >= _elements.Length - 1)
					AddTreeLevel();
			}
			_elements[index] = element;
		}

		public T Search(T element)
		{
			if (_comparator.IsNull(element))
				throw new ArgumentNullException(nameof(element));

			int index = 0;
			ComparisonResult direction;
			for (; (direction = _comparator.Compare(element, _elements[index])) != ComparisonResult.Equal;)
			{
				if (direction == ComparisonResult.Less)
					index = (index << 1) + 1;
				else if (direction == ComparisonResult.More)
					index = (index << 1) + 2;

				if (index >= _elements.Length - 1)
					return _comparator.NullValue;
			}
			return _elements[index];
		}

		private void AddTreeLevel()
		{
			if (Level == MaxLevel)
				throw new Exception("The maximum tree level was reached.");
			T[] newElements = new T[1 << Level + 1];
			Array.Copy(_elements, 0, newElements, 0, _elements.Length);
			FillNullValues(newElements, _elements.Length, newElements.Length - _elements.Length);
			_elements = newElements;
		}
		private void FillNullValues(T[] elements, int offset, int count)
		{
			for (int i = 0; i != count; i++)
				elements[i + offset] = _comparator.NullValue;
		}
	}
}
