using Devdeb.Sets.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Devdeb.Storage
{
	public class StorableHeap
	{
		internal class SegmentsInformation
		{
			private readonly List<Segment> _usedSegments;
			private readonly RedBlackTreeSurjection<long, Segment> _freeSegmentsPointers;
			private readonly RedBlackTreeSurjection<long, Queue<Segment>> _freeSegmentsSizes;

			public SegmentsInformation(Segment[] freeSegments, Segment[] usedSegments)
			{
				if (freeSegments == null)
					throw new ArgumentNullException(nameof(freeSegments));

				_usedSegments = usedSegments == null ? new List<Segment>() : usedSegments.ToList();
				_freeSegmentsPointers = new RedBlackTreeSurjection<long, Segment>();
				_freeSegmentsSizes = new RedBlackTreeSurjection<long, Queue<Segment>>();

				foreach (Segment segment in freeSegments)
					AddFreeSegment(segment);
			}

			public List<Segment> Used => _usedSegments;
			public RedBlackTreeSurjection<long, Segment> FreePointers => _freeSegmentsPointers;
			public RedBlackTreeSurjection<long, Queue<Segment>> FreeSizes => _freeSegmentsSizes;
			public long FreeSize => FreePointers.Select(x => x.Output.Size).Sum();

			public void AddFreeSegment(Segment segment)
			{
				_freeSegmentsPointers.Add(segment.Pointer, segment);
				if (_freeSegmentsSizes.TryGetValue(segment.Size, out Queue<Segment> segments))
					segments.Enqueue(segment);
				else
					_freeSegmentsSizes.Add(segment.Size, new Queue<Segment>(new Segment[] { segment }));
			}
			public void FreeSegment(Segment segment)
			{
				Debug.Assert(_usedSegments.Remove(segment), $"The {nameof(segment)} was removed.");
				_freeSegmentsPointers.Add(segment.Pointer, segment);
				if (_freeSegmentsSizes.TryGetValue(segment.Size, out Queue<Segment> segments))
					segments.Enqueue(segment);
				else
					_freeSegmentsSizes.Add(segment.Size, new Queue<Segment>(new Segment[] { segment }));
			}
		}

		private const long DefaultInitializedHeapSize = 4096;
		private const string HeapFileName = "_data";
		private const string FreeSegmentsFileName = "_segments";

		private readonly long _maxHeapSize;
		private long _currentHeapSize;
		private readonly object _currentHeapSizeLock;
		private readonly DirectoryInfo _heapDirectory;
		private readonly SegmentsInformation _segments;

		public StorableHeap(DirectoryInfo heapDirectory, long maxHeapSize)
		{
			if (heapDirectory == null)
				throw new ArgumentNullException(nameof(heapDirectory));
			if (maxHeapSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxHeapSize));

			_heapDirectory = heapDirectory;
			_maxHeapSize = maxHeapSize;
			_currentHeapSizeLock = new object();

			if (TryLoadSegments(out Segment[] freeSegments, out Segment[] usedSegments))
			{
				using FileStream fileStream = File.Open(HeapFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				_currentHeapSize = fileStream.Length - 1;
				_segments = new SegmentsInformation(freeSegments, usedSegments);
			}
			else
			{
				_currentHeapSize = DefaultInitializedHeapSize;
				freeSegments = new Segment[]
				{
					new Segment
					{
						Pointer = 0,
						Size = _currentHeapSize
					}
				};
				_segments = new SegmentsInformation(freeSegments, null);
				UploadSegments();
				using FileStream fileStream = File.Open(HeapFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
				_ = fileStream.Seek(_currentHeapSize, SeekOrigin.Begin);
				fileStream.WriteByte(0);
				fileStream.Flush();
			}
		}

		protected string HeapFilePath => Path.Combine(_heapDirectory.FullName, HeapFileName);
		protected string FreeSegmentsFilePath => Path.Combine(_heapDirectory.FullName, FreeSegmentsFileName);
		public long UsedSize => _currentHeapSize - _segments.FreeSize;

		public Segment AllocateMemory(long size)
		{
			if (size <= 0)
				throw new ArgumentOutOfRangeException(nameof(size));

			Segment segment = default;
			if (_segments.FreeSizes.TryGetMin(size, out Queue<Segment> segments))
			{
				Segment freeSegment = segments.Dequeue();
				Debug.Assert(_segments.FreePointers.Remove(freeSegment.Pointer));
				if (segments.Count == 0)
					Debug.Assert(_segments.FreeSizes.Remove(freeSegment.Size));

				if (freeSegment.Size == size)
				{
					segment = freeSegment;
					_segments.Used.Add(segment);
					UploadSegments();
					return segment;
				}

				segment = new Segment
				{
					Pointer = freeSegment.Pointer,
					Size = size
				};

				freeSegment.Pointer += size;
				freeSegment.Size -= size;
				_segments.AddFreeSegment(freeSegment);
				_segments.Used.Add(segment);
				UploadSegments();
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

					using FileStream fileStream = File.Open(HeapFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
					_ = fileStream.Seek(_currentHeapSize, SeekOrigin.Begin);
					fileStream.WriteByte(0);
					fileStream.Flush();
				}
				_segments.Used.Add(segment);
				UploadSegments();
				return segment;
			}
		}
		public void FreeMemory(Segment segment)
		{
			_segments.FreeSegment(segment);
			UploadSegments();
		}
		public void Defragment()
		{
			_segments.Used.Sort((x, y) => Comparer<long>.Default.Compare(x.Pointer, y.Pointer));
			long offset = 0;
			using FileStream fileStream = File.Open(HeapFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			for (int i = 0; i < _segments.Used.Count; i++)
			{
				Segment segment = _segments.Used[i];
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
				_segments.Used[i] = segment;
				offset += segment.Size;
			}
			_segments.FreePointers.Clear();
			_segments.FreeSizes.Clear();
			Segment remainedSegment = new Segment
			{
				Pointer = offset,
				Size = _currentHeapSize - offset
			};
			_segments.AddFreeSegment(remainedSegment);
			UploadSegments();
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
			if (!_segments.Used.Contains(segment))
				throw new Exception($"The {nameof(segment)} isn't contained in used segments of heap.");

			using FileStream fileStream = File.Open(HeapFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
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
			if (!_segments.Used.Contains(segment))
				throw new Exception($"The {nameof(segment)} isn't contained in used segments of heap.");


			using FileStream fileStream = File.Open(HeapFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			_ = fileStream.Seek(segment.Pointer, SeekOrigin.Begin);
			int readCount = fileStream.Read(buffer, offset, count);
			if (readCount != count)
				throw new Exception($"The number of bytes read from the file does not match {nameof(count)}");
		}

		private unsafe void UploadSegments()
		{
			Segment[] freeSegments = _segments.FreePointers.Select(x => x.Output).ToArray();
			Segment[] usedSegments = _segments.Used.ToArray();
			byte[] freeSegmentsBuffer = new SegmentArraySerializer().Serialize(freeSegments);
			byte[] usedSegmentsBuffer = new SegmentArraySerializer().Serialize(usedSegments);

			using FileStream fileStream = File.Open(FreeSegmentsFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
			_ = fileStream.Seek(0, SeekOrigin.Begin);
			fileStream.Write(freeSegmentsBuffer, 0, freeSegmentsBuffer.Length);
			fileStream.Write(usedSegmentsBuffer, 0, usedSegmentsBuffer.Length);
			fileStream.Flush();
		}
		private unsafe bool TryLoadSegments(out Segment[] freeSegments, out Segment[] usedSegments)
		{
			using FileStream fileStream = File.Open(FreeSegmentsFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
			if (fileStream.Length < SegmentArraySerializer.ArrayLengthBytesCount)
			{
				freeSegments = default;
				usedSegments = default;
				return false;
			}
			long offset = 0;
			freeSegments = loadSegments(ref offset);
			usedSegments = loadSegments(ref offset);
			return true;

			unsafe Segment[] loadSegments(ref long offset)
			{
				using FileStream fileStream = File.Open(FreeSegmentsFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
				_ = fileStream.Seek(offset, SeekOrigin.Begin);

				byte[] arrayLengthBytes = new byte[SegmentArraySerializer.ArrayLengthBytesCount];
				int readCount = fileStream.Read(arrayLengthBytes, 0, arrayLengthBytes.Length);
				if (readCount != arrayLengthBytes.Length)
					throw new Exception($"The number of bytes for array length read from the file doesn't match {nameof(arrayLengthBytes.Length)}");

				int arrayLength;
				fixed (byte* arrayLengthBytesPointer = &arrayLengthBytes[0])
					arrayLength = *(int*)arrayLengthBytesPointer;

				int arrayDataBytesCount = arrayLength * SegmentArraySerializer.SegmentLengthBytesCount;
				byte[] buffer = new byte[SegmentArraySerializer.ArrayLengthBytesCount + arrayDataBytesCount];
				Array.Copy(arrayLengthBytes, 0, buffer, 0, arrayLengthBytes.Length);
				readCount = fileStream.Read(buffer, SegmentArraySerializer.ArrayLengthBytesCount, arrayDataBytesCount);
				if (readCount != arrayDataBytesCount)
					throw new Exception($"The number of bytes for segments buffer read from the file doesn't match {nameof(arrayDataBytesCount)}");
				offset = fileStream.Position;
				return new SegmentArraySerializer().Deserialize(buffer);
			}
		}
	}
}
