using Devdeb.Sets.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Devdeb.Storage
{
	public class StorableHeap
	{
		private const long DefaultInitializedHeapSize = 4096;
		private const string HeapFileName = "_data";

		private readonly List<Segment> _usedSegments;
		private readonly RedBlackTreeSurjection<long, Segment> _freeSegmentsPointers;
		private readonly RedBlackTreeSurjection<long, Queue<Segment>> _freeSegmentsSizes;
		private readonly long _maxHeapSize;
		private long _currentHeapSize;
		private readonly object _currentHeapSizeLock;
		private readonly DirectoryInfo _heapDirectory;

		protected string FilePath => Path.Combine(_heapDirectory.FullName, HeapFileName);

		public StorableHeap(DirectoryInfo heapDirectory, long maxHeapSize)
		{
			if (heapDirectory == null)
				throw new ArgumentNullException(nameof(heapDirectory));
			if (maxHeapSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxHeapSize));

			_heapDirectory = heapDirectory;
			_maxHeapSize = maxHeapSize;
			_currentHeapSize = DefaultInitializedHeapSize;
			_currentHeapSizeLock = new object();
			_usedSegments = new List<Segment>();

			Segment segment = new Segment
			{
				Pointer = 0,
				Size = _currentHeapSize
			};
			_freeSegmentsPointers = new RedBlackTreeSurjection<long, Segment>();
			_freeSegmentsSizes = new RedBlackTreeSurjection<long, Queue<Segment>>();
			_freeSegmentsPointers.Add(segment.Pointer, segment);
			_freeSegmentsSizes.Add(segment.Size, new Queue<Segment>(new Segment[] { segment }));

			using FileStream fileStream = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
			fileStream.Seek(_currentHeapSize + 1, SeekOrigin.Begin);
			fileStream.WriteByte(0);
			fileStream.Flush();
		}

		public Segment AllocateMemory(long size)
		{
			if (size <= 0)
				throw new ArgumentOutOfRangeException(nameof(size));

			Segment segment = default;
			if (_freeSegmentsSizes.TryGetMin(size, out Queue<Segment> segments))
			{
				Segment freeSegment = segments.Dequeue();
				Debug.Assert(_freeSegmentsPointers.Remove(freeSegment.Pointer));
				if (segments.Count == 0)
					Debug.Assert(_freeSegmentsSizes.Remove(freeSegment.Size));

				if (freeSegment.Size == size)
				{
					segment = freeSegment;
					_usedSegments.Add(segment);
					return segment;
				}

				segment = new Segment
				{
					Pointer = freeSegment.Pointer,
					Size = size
				};

				freeSegment.Pointer += size;
				freeSegment.Size -= size;
				_freeSegmentsPointers.Add(freeSegment.Pointer, freeSegment);
				if (_freeSegmentsSizes.TryGetValue(freeSegment.Size, out segments))
					segments.Enqueue(freeSegment);
				else
					_freeSegmentsSizes.Add(freeSegment.Size, new Queue<Segment>(new Segment[] { freeSegment }));

				_usedSegments.Add(segment);
				return segment;
			}
			else
			{
				//add defragmentation

				lock (_currentHeapSizeLock)
				{
					if (_currentHeapSize + size > _maxHeapSize)
						throw new Exception("Requested size exceeds free space.");

					segment = new Segment
					{
						Pointer = _currentHeapSize,
						Size = size
					};
					_currentHeapSize += size;

					using FileStream fileStream = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
					_ = fileStream.Seek(_currentHeapSize + 1, SeekOrigin.Begin);
					fileStream.WriteByte(0);
					fileStream.Flush();
				}
				_usedSegments.Add(segment);
				return segment;
			}
		}
		public void FreeMemory(Segment segment)
		{
			if (!_usedSegments.Remove(segment))
				throw new Exception($"The {nameof(segment)} was removed.");
			_freeSegmentsPointers.Add(segment.Pointer, segment);
			if (_freeSegmentsSizes.TryGetValue(segment.Size, out Queue<Segment> segments))
				segments.Enqueue(segment);
			else
				_freeSegmentsSizes.Add(segment.Size, new Queue<Segment>(new Segment[] { segment }));
		}
		public void Defragment()
		{
			_usedSegments.Sort((x, y) => Comparer<long>.Default.Compare(x.Pointer, y.Pointer));
			long offset = 0;
			using FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			for (int i = 0; i < _usedSegments.Count; i++)
			{
				Segment segment = _usedSegments[i];
				if (segment.Pointer == offset)
				{
					offset += segment.Size;
					continue;
				}

				byte[] segmentData = new byte[segment.Size];
				_ = fileStream.Seek(segment.Pointer, SeekOrigin.Begin);
				int readCount = fileStream.Read(segmentData, 0, checked((int)segment.Size));
				if (readCount != segmentData.Length)
					throw new Exception($"The number of bytes read from the file does not match the segment size.");

				_ = fileStream.Seek(offset, SeekOrigin.Begin);
				fileStream.Write(segmentData, 0, segmentData.Length);
				fileStream.Flush();

				segment.Pointer = offset;
				_usedSegments[i] = segment;
				offset += segment.Size;
			}
			_freeSegmentsPointers.Clear();
			_freeSegmentsSizes.Clear();
			Segment remainedSegment = new Segment
			{
				Pointer = offset,
				Size = _currentHeapSize - offset
			};
			_freeSegmentsPointers.Add(remainedSegment.Pointer, remainedSegment);
			_freeSegmentsSizes.Add(remainedSegment.Size, new Queue<Segment>(new Segment[] { remainedSegment }));
		}

		public void Write(Segment segment, byte[] buffer, int offset, int count)
		{
			//perhaps add segmentOffset
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count <= 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > buffer.Length)
				throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}");
			if (count > segment.Size)
				throw new Exception($"The {nameof(count)} exceeds {nameof(segment.Size)}");
			if (!_usedSegments.Contains(segment))
				throw new Exception($"The {nameof(segment)} isn't contained in used segments of heap.");

			using FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
			_ = fileStream.Seek(segment.Pointer, SeekOrigin.Begin);
			fileStream.Write(buffer, offset, count);
			fileStream.Flush();
		}
		public void ReadBytes(Segment segment, byte[] buffer, int offset, int count)
		{
			//perhaps add segmentOffset
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count <= 0)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > buffer.Length)
				throw new Exception($"The {nameof(offset)} with {nameof(count)} exceeds {nameof(buffer.Length)}");
			if (count > segment.Size)
				throw new Exception($"The {nameof(count)} exceeds {nameof(segment.Size)}");
			if (!_usedSegments.Contains(segment))
				throw new Exception($"The {nameof(segment)} isn't contained in used segments of heap.");


			using FileStream fileStream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			_ = fileStream.Seek(segment.Pointer, SeekOrigin.Begin);
			int readCount = fileStream.Read(buffer, offset, count);
			if (readCount != count)
				throw new Exception($"The number of bytes read from the file does not match {nameof(count)}");
		}
	}
}
