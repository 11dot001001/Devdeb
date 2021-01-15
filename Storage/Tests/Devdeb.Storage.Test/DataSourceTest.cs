using Devdeb.Sets.Extensions;
using Devdeb.Sets.Generic;
using Devdeb.Sets.Ratios;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Devdeb.Storage.Test.DataSourceTests
{
	internal class DataSourceTest
	{
		public const string DatabaseDirectory = @"C:\Users\lehac\Desktop\data";
		public const long MaxHeapSize = 10000;
		public DirectoryInfo DatabaseDirectoryInfo => new DirectoryInfo(DatabaseDirectory);

		public void Test()
		{
			DataSource dataSource = new DataSource(DatabaseDirectoryInfo, MaxHeapSize);

			StoredClass storedClass1 = new StoredClass() //1004 21.
			{
				Id = 15,
				Value = "StoredClass 1"
			};
			StoredClass storedClass2 = new StoredClass() //1025 21.
			{
				Id = 435234523,
				Value = "StoredClass 1"
			};
			StoredClass storedClass3 = new StoredClass() //1046 21.
			{
				Id = 1,
				Value = "StoredClass 1"
			};
			dataSource.Add(storedClass1);
			dataSource.Add(storedClass2);
			dataSource.Add(storedClass3);
			dataSource.RemoveById(15);
			bool is0WasFound = dataSource.TryGetById(0, out StoredClass storedClass0);
			bool is15WasFound = dataSource.TryGetById(15, out StoredClass storedClass15);
			bool is435234523WasFound = dataSource.TryGetById(435234523, out StoredClass storedClass435234523);
		}

		public class StoredClass
		{
			public int Id { get; set; }
			public string Value { get; set; }
		}
		public class StoredClassSerializer
		{
			public int Size(StoredClass instance)
			{
				const int idSize = sizeof(int);
				const int valueLengthSize = sizeof(int);
				return idSize + valueLengthSize + Encoding.UTF8.GetByteCount(instance.Value);
			}
			public unsafe void Serialize(StoredClass instance, byte[] buffer, int offset)
			{
				int instanceSize = Size(instance);
				ArrayExtensions.EnsureLength(ref buffer, offset + instanceSize);

				fixed (byte* bufferPointer = &buffer[offset])
				{
					*(int*)bufferPointer = instance.Id;
					*((int*)bufferPointer + 1) = Encoding.UTF8.GetByteCount(instance.Value);
				}
				Encoding.UTF8.GetBytes(instance.Value, 0, instance.Value.Length, buffer, offset + 8);
			}
			public unsafe StoredClass Deserialize(byte[] buffer, int offset)
			{
				StoredClass instance = new StoredClass();
				int valueLength;
				fixed (byte* bufferPointer = &buffer[offset])
				{
					instance.Id = *(int*)bufferPointer;
					valueLength = *((int*)bufferPointer + 1);
				}
				instance.Value = Encoding.UTF8.GetString(buffer, offset + 8, valueLength);
				return instance;
			}
		}
		public class Meta
		{
			private readonly RedBlackTreeSurjection<int, Segment> _storedClassPrimaryIndexes;

			public Meta() : this(new RedBlackTreeSurjection<int, Segment>()) { }
			public Meta(RedBlackTreeSurjection<int, Segment> storedClassPrimaryIndexes)
			{
				_storedClassPrimaryIndexes = storedClassPrimaryIndexes ?? throw new ArgumentNullException(nameof(storedClassPrimaryIndexes));
			}

			public RedBlackTreeSurjection<int, Segment> StoredClassPrimaryIndexes => _storedClassPrimaryIndexes;
		}
		public class MetaSerializer
		{
			private class IndexSerializer
			{
				public int Size() => sizeof(int) + 2 * sizeof(long);
				public unsafe void Serialize(SurjectionRatio<int, Segment> instance, byte[] buffer, int offset)
				{
					ArrayExtensions.EnsureLength(ref buffer, offset + Size());
					fixed (byte* bufferPointer = &buffer[offset])
					{
						*(int*)bufferPointer = instance.Input;
						*(long*)((int*)bufferPointer + 1) = instance.Output.Pointer;
						*((long*)((int*)bufferPointer + 1) + 1) = instance.Output.Size;
					}
				}
				public unsafe SurjectionRatio<int, Segment> Deserialize(byte[] buffer, int offset)
				{
					int id;
					Segment segment = new Segment();
					fixed (byte* bufferPointer = &buffer[offset])
					{
						id = *(int*)bufferPointer;
						segment.Pointer = *(long*)((int*)bufferPointer + 1);
						segment.Size = *((long*)((int*)bufferPointer + 1) + 1);
					}
					return new SurjectionRatio<int, Segment>(id, segment);
				}
			}

			private const int ArrayLengthSize = sizeof(int);

			private readonly IndexSerializer _indexSerializer;

			public MetaSerializer() => _indexSerializer = new IndexSerializer();

			public int Size(Meta instance)
			{
				return ArrayLengthSize + (instance.StoredClassPrimaryIndexes.Count * _indexSerializer.Size());
			}
			public unsafe void Serialize(Meta instance, byte[] buffer, int offset)
			{
				SurjectionRatio<int, Segment>[] indexes = instance.StoredClassPrimaryIndexes.ToArray();
				fixed (byte* bufferPointer = &buffer[offset])
					*(int*)bufferPointer = indexes.Length;
				offset += ArrayLengthSize;
				for (int i = 0; i < indexes.Length; i++)
				{ 
					_indexSerializer.Serialize(indexes[i], buffer, offset);
					offset += _indexSerializer.Size();
				}
			}
			public unsafe Meta Deserialize(byte[] buffer, int offset)
			{
				int arrayLength;
				fixed (byte* bufferPointer = &buffer[offset])
					arrayLength = *(int*)bufferPointer;
				offset += ArrayLengthSize;
				SurjectionRatio<int, Segment>[] indexes = new SurjectionRatio<int, Segment>[arrayLength];
				for (int i = 0; i != indexes.Length; i++)
				{ 
					indexes[i] = _indexSerializer.Deserialize(buffer, offset);
					offset += _indexSerializer.Size();
				}
				return new Meta(new RedBlackTreeSurjection<int, Segment>(indexes));
			}
		}

		public class DataSource
		{
			private readonly StorableHeap _storableHeap;
			private readonly StoredClassSerializer _storedClassSerializer;
			private readonly Meta _meta;

			public DataSource(DirectoryInfo heapDirectory, long maxHeapSize)
			{
				_storableHeap = new StorableHeap(heapDirectory, maxHeapSize);
				_storedClassSerializer = new StoredClassSerializer();
				_meta = InitializeMeta();
			}

			public bool Add(StoredClass instance)
			{
				int instanceLength = _storedClassSerializer.Size(instance);
				Segment instanceSegment = _storableHeap.AllocateMemory(instanceLength);

				if (!_meta.StoredClassPrimaryIndexes.TryAdd(instance.Id, instanceSegment))
					return false;

				byte[] buffer = new byte[_storedClassSerializer.Size(instance)];
				_storedClassSerializer.Serialize(instance, buffer, 0);
				_storableHeap.Write(instanceSegment, buffer, 0, buffer.Length);
				UploadMeta(_meta, false);
				return true;
			}
			public bool TryGetById(int id, out StoredClass instance)
			{
				if (!_meta.StoredClassPrimaryIndexes.TryGetValue(id, out Segment segment))
				{
					instance = default;
					return false;
				}

				byte[] buffer = new byte[segment.Size]; //segment.Size incredible crutch may be.
				_storableHeap.ReadBytes(segment, buffer, 0, buffer.Length);
				instance =_storedClassSerializer.Deserialize(buffer, 0);
				return true;
			}
			public void RemoveById(int id)
			{
				if (!_meta.StoredClassPrimaryIndexes.Remove(id, out Segment segment))
					return;
				_storableHeap.FreeMemory(segment);
				UploadMeta(_meta, false);
			}

			private unsafe Meta InitializeMeta()
			{
				if (!_storableHeap.IsInitializationFirst)
					return LoadMeta();
				Meta meta = new Meta();
				UploadMeta(meta, true);
				return meta;
			}
			private unsafe Meta LoadMeta()
			{
				byte[] metaLengthBuffer = new byte[MetaLengthSize];
				_storableHeap.ReadBytes(MetaLengthSegment, metaLengthBuffer, 0, metaLengthBuffer.Length);
				int metaLength;
				fixed (byte* metaLengthBufferPointer = &metaLengthBuffer[0])
					metaLength = *(int*)metaLengthBufferPointer;

				byte[] metaBuffer = new byte[metaLength];
				_storableHeap.ReadBytes(GetMetaSegment(MetaCapacity), metaBuffer, 0, metaBuffer.Length);
				return new MetaSerializer().Deserialize(metaBuffer, 0);
			}
			private unsafe void UploadMeta(Meta meta, bool isInitialization)
			{
				MetaSerializer metaSerializer = new MetaSerializer();

				if (isInitialization)
					Debug.Assert(_storableHeap.AllocateMemory(MetaLengthSize) == MetaLengthSegment);

				int metaLength = metaSerializer.Size(meta);
				byte[] metaLengthBuffer = new byte[MetaLengthSize];
				fixed (byte* metaLengthBufferPointer = &metaLengthBuffer[0])
					*(int*)metaLengthBufferPointer = metaLength;
				_storableHeap.Write(MetaLengthSegment, metaLengthBuffer, 0, metaLengthBuffer.Length);

				if (metaLength > MetaCapacity)
					throw new Exception("Temporary restriction for meta length.");
				Segment metaSegment = GetMetaSegment(metaLength);
				if (isInitialization)
					_storableHeap.AllocateMemory(MetaCapacity);
				//Debug.Assert(_storableHeap.AllocateMemory(MetaCapacity) == metaSegment); //while above restriction
				byte[] metaBuffer = new byte[metaLength];
				metaSerializer.Serialize(meta, metaBuffer, 0);
				_storableHeap.Write(GetMetaSegment(MetaCapacity), metaBuffer, 0, metaBuffer.Length);
			}

			private const int MetaLengthSize = sizeof(int);
			private Segment MetaLengthSegment => new Segment
			{
				Pointer = 0,
				Size = MetaLengthSize
			};
			private Segment GetMetaSegment(int metaLength) => new Segment
			{
				Pointer = MetaLengthSize,
				Size = metaLength
			};
			private const int MetaCapacity = 1000;
		}
	}
}
