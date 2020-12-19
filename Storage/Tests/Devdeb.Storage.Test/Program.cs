using System;
using System.IO;

namespace Devdeb.Storage.Test
{
	class Program
	{
		public const string DatabaseDirectory = @"C:\Users\lehac\Desktop\data";
		public const long MaxHeapSize = 10000;

		static void Main(string[] args)
		{
			BinaryTree<int> binaryTree = new BinaryTree<int>(20, new IntComparator(int.MinValue));
			binaryTree.Add(1);

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
	}

	public class IntComparator : IBinaryTreeComparator<int>
	{
		private readonly int _nullValue;

		public IntComparator(int nullValue) => _nullValue = nullValue;

		public ComparisonResult Compare(int value1, int value2)
		{
			int comparisonResult = value1.CompareTo(value2);
			if (comparisonResult == 0)
				return ComparisonResult.Equal;
			else if(comparisonResult > 0)
				return ComparisonResult.More;
			else if (comparisonResult < 0)
				return ComparisonResult.Less;
			throw new Exception();
		}

		public bool IsNull(int value) => _nullValue == value;
	}

	public class BinaryTree<T>
	{
		public const int MaxLevel = 30;
		public const int MinLevel = 1;

		private T[] _elements;
		private readonly IBinaryTreeComparator<T> _comparator;

		public BinaryTree(int levels, IBinaryTreeComparator<T> comparator)
		{
			if (levels < MinLevel || levels > MaxLevel)
				throw new ArgumentOutOfRangeException(nameof(levels));
			if (comparator == null)
				throw new ArgumentNullException(nameof(comparator));

			_elements = new T[1 << levels];
			_comparator = comparator;
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

		public void Add(T element)
		{

		}

		public T Search(T element)
		{
			return default;
		}

		private void AddTreeLevel()
		{
			if (Level == MaxLevel)
				throw new Exception("The maximum tree level was reached.");
			T[] newElements = new T[1 << Level + 1];
			Array.Copy(_elements, 0, newElements, 0, _elements.Length);
			_elements = newElements;
		}
	}
}
