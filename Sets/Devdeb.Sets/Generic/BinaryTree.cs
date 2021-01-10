using System;

namespace Devdeb.Sets.Generic
{
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
			_comparator = comparator ?? throw new ArgumentNullException(nameof(comparator));
			_elements = new T[1 << levels];
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
