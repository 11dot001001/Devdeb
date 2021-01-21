using Devdeb.Sets.Extensions;
using Devdeb.Sets.Generic;
using Devdeb.Sets.Ratios;
using Devdeb.Storage.Serializers;
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

			StoredClass[] startStoredClasses = dataSource.StoredClassSet.GetAll();

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
			dataSource.StoredClassSet.Add(storedClass1);
			dataSource.StoredClassSet.Add(storedClass2);
			dataSource.StoredClassSet.Add(storedClass3);
			dataSource.StoredClassSet.RemoveById(15);
			bool is0WasFound = dataSource.StoredClassSet.TryGetById(0, out StoredClass storedClass0);
			bool is15WasFound = dataSource.StoredClassSet.TryGetById(15, out StoredClass storedClass15);
			bool is435234523WasFound = dataSource.StoredClassSet.TryGetById(435234523, out StoredClass storedClass435234523);

			StoredClass[] storedClasses = dataSource.StoredClassSet.GetAll();
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
			private readonly DataSetMeta _storedClassSetMeta;

			public Meta() : this(new DataSetMeta()) { }
			public Meta(DataSetMeta storedClassSetMeta)
			{
				_storedClassSetMeta = storedClassSetMeta ?? throw new ArgumentNullException(nameof(storedClassSetMeta));
			}

			public DataSetMeta StoredClassSetMeta => _storedClassSetMeta;
		}
		public class MetaSerializer
		{
			private readonly DataSetMetaSerializer _storedClassDataSetMetaSerializer;

			public MetaSerializer() => _storedClassDataSetMetaSerializer = new DataSetMetaSerializer();

			public int Size(Meta instance) => _storedClassDataSetMetaSerializer.Size();
			public unsafe void Serialize(Meta instance, byte[] buffer, int offset)
			{
				_storedClassDataSetMetaSerializer.Serialize(instance.StoredClassSetMeta, buffer, offset);
			}
			public unsafe Meta Deserialize(byte[] buffer, int offset)
			{
				DataSetMeta storedClassDataSetMeta = _storedClassDataSetMetaSerializer.Deserialize(buffer, offset);
				return new Meta(storedClassDataSetMeta);
			}
		}

		public class DataSource
		{
			private readonly StorableHeap _storableHeap;
			private readonly MetaSerializer _metaSerializer;
			private readonly SegmentSerializer _segmentSerializer;
			private readonly Meta _meta;
			private readonly DataSet<StoredClass> _storedClassSet;

			public DataSource(DirectoryInfo heapDirectory, long maxHeapSize)
			{
				_storableHeap = new StorableHeap(heapDirectory, maxHeapSize);
				_metaSerializer = new MetaSerializer();
				_segmentSerializer = new SegmentSerializer();
				_meta = InitializeMeta();
				_storedClassSet = InitializeStoredClassSet();
			}

			public DataSet<StoredClass> StoredClassSet => _storedClassSet;

			private unsafe DataSet<StoredClass> InitializeStoredClassSet()
			{
				Debug.Assert(_meta != default);
				if (!_storableHeap.IsInitializationFirst)
					return LoadStoredClassSet();
				DataSet<StoredClass> storedClassSet = new DataSet<StoredClass>
				(
					_storableHeap,
					this,
					_meta.StoredClassSetMeta,
					new RedBlackTreeSurjection<int, Segment>()
				);
				DataSet<StoredClass>.UploadDataSet(storedClassSet);
				return storedClassSet;
			}
			private DataSet<StoredClass> LoadStoredClassSet()
			{
				Debug.Assert(_meta.StoredClassSetMeta != default);
				RedBlackTreeSurjection<int, Segment> primaryIndexes = DataSet<StoredClass>.LoadPrimaryIndexes(_storableHeap, _meta.StoredClassSetMeta);
				return new DataSet<StoredClass>(_storableHeap, this, _meta.StoredClassSetMeta, primaryIndexes);
			}

			private Meta InitializeMeta()
			{
				if (!_storableHeap.IsInitializationFirst)
					return LoadMeta();
				InitializeMetaSegment();
				Meta meta = new Meta();
				UploadMeta(meta);
				return meta;
			}
			private unsafe Meta LoadMeta()
			{
				Segment metaSegment = LoadMetaSegment();
				Debug.Assert(metaSegment != default);
				byte[] buffer = new byte[metaSegment.Size];
				_storableHeap.ReadBytes(metaSegment, buffer, 0, buffer.Length);
				return _metaSerializer.Deserialize(buffer, 0);
			}
			private unsafe void UploadMeta(Meta meta)
			{
				int metaLength = _metaSerializer.Size(meta);
				Segment metaSegment = LoadMetaSegment();
				if (metaLength != metaSegment.Size)
				{
					if (metaSegment != default)
						_storableHeap.FreeMemory(metaSegment);
					metaSegment = _storableHeap.AllocateMemory(metaLength);
					UploadMetaSegment(metaSegment);
				}

				byte[] buffer = new byte[metaLength];
				_metaSerializer.Serialize(meta, buffer, 0);
				_storableHeap.Write(metaSegment, buffer, 0, buffer.Length);
			}
			internal void UploadMeta() => UploadMeta(_meta);

			private Segment InitializaionSegment => new Segment
			{
				Pointer = 0,
				Size = _segmentSerializer.Size()
			};
			private void InitializeMetaSegment()
			{
				Debug.Assert(_storableHeap.AllocateMemory(_segmentSerializer.Size()) == InitializaionSegment);
				UploadMetaSegment(default);
			}
			private Segment LoadMetaSegment()
			{
				byte[] buffer = new byte[_segmentSerializer.Size()];
				_storableHeap.ReadBytes(InitializaionSegment, buffer, 0, buffer.Length);
				return _segmentSerializer.Deserialize(buffer, 0);
			}
			private void UploadMetaSegment(Segment segment)
			{
				byte[] buffer = new byte[_segmentSerializer.Size()];
				_segmentSerializer.Serialize(segment, buffer, 0);
				_storableHeap.Write(InitializaionSegment, buffer, 0, buffer.Length);
			}
		}

		public class DataSetMeta
		{
			public Segment PrimaryIndexesPointer { get; set; }
		}
		public class DataSetMetaSerializer
		{
			private readonly SegmentSerializer _segmentSerializer = new SegmentSerializer();

			public int Size() => _segmentSerializer.Size();
			public unsafe void Serialize(DataSetMeta instance, byte[] buffer, int offset)
			{
				_segmentSerializer.Serialize(instance.PrimaryIndexesPointer, buffer, offset);
			}
			public unsafe DataSetMeta Deserialize(byte[] buffer, int offset)
			{
				Segment segment = _segmentSerializer.Deserialize(buffer, offset);
				return new DataSetMeta { PrimaryIndexesPointer = segment };
			}
		}

		public class IndexSerializer
		{
			private readonly SegmentSerializer _segmentSerializer = new SegmentSerializer();

			public int Size() => sizeof(int) + _segmentSerializer.Size();
			public unsafe void Serialize(SurjectionRatio<int, Segment> instance, byte[] buffer, int offset)
			{
				ArrayExtensions.EnsureLength(ref buffer, offset + Size());
				fixed (byte* bufferPointer = &buffer[offset])
					*(int*)bufferPointer = instance.Input;
				_segmentSerializer.Serialize(instance.Output, buffer, offset += sizeof(int));
			}
			public unsafe SurjectionRatio<int, Segment> Deserialize(byte[] buffer, int offset)
			{
				int id;
				fixed (byte* bufferPointer = &buffer[offset])
					id = *(int*)bufferPointer;
				Segment segment = _segmentSerializer.Deserialize(buffer, offset += sizeof(int));
				return new SurjectionRatio<int, Segment>(id, segment);
			}
		}
		public class IndexSurjectionSerializer
		{
			private const int ArrayLengthSize = sizeof(int);

			private readonly IndexSerializer _indexSerializer;

			public IndexSurjectionSerializer() => _indexSerializer = new IndexSerializer();

			public int Size(RedBlackTreeSurjection<int, Segment> instance)
			{
				return ArrayLengthSize + (instance.Count * _indexSerializer.Size());
			}
			public unsafe void Serialize(RedBlackTreeSurjection<int, Segment> instance, byte[] buffer, int offset)
			{
				SurjectionRatio<int, Segment>[] indexes = instance.ToArray();
				fixed (byte* bufferPointer = &buffer[offset])
					*(int*)bufferPointer = indexes.Length;
				offset += ArrayLengthSize;
				for (int i = 0; i < indexes.Length; i++)
				{
					_indexSerializer.Serialize(indexes[i], buffer, offset);
					offset += _indexSerializer.Size();
				}
			}
			public unsafe RedBlackTreeSurjection<int, Segment> Deserialize(byte[] buffer, int offset)
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
				return new RedBlackTreeSurjection<int, Segment>(indexes);
			}
		}

		public class DataSet<T>
		{
			static private readonly IndexSurjectionSerializer _indexSurjectionSerializer;
			static private readonly StoredClassSerializer _entitySerializer;

			static DataSet()
			{
				_entitySerializer = new StoredClassSerializer();
				_indexSurjectionSerializer = new IndexSurjectionSerializer();
			}

			static internal RedBlackTreeSurjection<int, Segment> LoadPrimaryIndexes(StorableHeap storableHeap, DataSetMeta meta)
			{
				Debug.Assert(meta.PrimaryIndexesPointer != default);
				byte[] buffer = new byte[meta.PrimaryIndexesPointer.Size];
				storableHeap.ReadBytes(meta.PrimaryIndexesPointer, buffer, 0, buffer.Length);
				return _indexSurjectionSerializer.Deserialize(buffer, 0);
			}
			static internal void UploadDataSet(DataSet<T> dataSet) => UploadPrimaryIndexes(dataSet);

			static private void UploadPrimaryIndexes(DataSet<T> dataSet)
			{
				int size = _indexSurjectionSerializer.Size(dataSet._primaryIndexes);
				if (size != dataSet._meta.PrimaryIndexesPointer.Size)
				{
					if (dataSet._meta.PrimaryIndexesPointer != default)
						dataSet._storableHeap.FreeMemory(dataSet._meta.PrimaryIndexesPointer);
					dataSet._meta.PrimaryIndexesPointer = dataSet._storableHeap.AllocateMemory(size);
					dataSet._dataSource.UploadMeta();
				}
				byte[] buffer = new byte[size];
				_indexSurjectionSerializer.Serialize(dataSet._primaryIndexes, buffer, 0);
				dataSet._storableHeap.Write(dataSet._meta.PrimaryIndexesPointer, buffer, 0, buffer.Length);
			}

			private readonly StorableHeap _storableHeap;
			private readonly DataSource _dataSource;
			private readonly DataSetMeta _meta;
			private readonly RedBlackTreeSurjection<int, Segment> _primaryIndexes;

			public DataSet(StorableHeap storableHeap, DataSource dataSource, DataSetMeta dataSetMeta, RedBlackTreeSurjection<int, Segment> primaryIndexes)
			{
				_storableHeap = storableHeap ?? throw new ArgumentNullException(nameof(storableHeap));
				_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
				_meta = dataSetMeta ?? throw new ArgumentNullException(nameof(dataSetMeta));
				_primaryIndexes = primaryIndexes ?? throw new ArgumentNullException(nameof(primaryIndexes));
			}

			public bool Add(StoredClass instance)
			{
				if (_primaryIndexes.TryGetValue(instance.Id, out _))
					return false;

				int instanceLength = _entitySerializer.Size(instance);
				Segment instanceSegment = _storableHeap.AllocateMemory(instanceLength);
				_primaryIndexes.Add(instance.Id, instanceSegment);

				byte[] buffer = new byte[instanceLength];
				_entitySerializer.Serialize(instance, buffer, 0);
				_storableHeap.Write(instanceSegment, buffer, 0, buffer.Length);
				UploadPrimaryIndexes(this);
				return true;
			}
			public bool TryGetById(int id, out StoredClass instance)
			{
				if (!_primaryIndexes.TryGetValue(id, out Segment segment))
				{
					instance = default;
					return false;
				}

				byte[] buffer = new byte[segment.Size]; //segment.Size incredible crutch may be.
				_storableHeap.ReadBytes(segment, buffer, 0, buffer.Length);
				instance = _entitySerializer.Deserialize(buffer, 0);
				return true;
			}
			public void RemoveById(int id)
			{
				if (!_primaryIndexes.Remove(id, out Segment segment))
					return;
				_storableHeap.FreeMemory(segment);
				UploadPrimaryIndexes(this);
			}
			public StoredClass[] GetAll()
			{
				Segment[] segments = _primaryIndexes.Select(x => x.Output).ToArray();
				StoredClass[] result = new StoredClass[segments.Length];
				for (int i = 0; i != result.Length; i++)
				{
					byte[] buffer = new byte[segments[i].Size]; //segment.Size incredible crutch may be.
					_storableHeap.ReadBytes(segments[i], buffer, 0, buffer.Length);
					result[i] = _entitySerializer.Deserialize(buffer, 0);
				}
				return result;
			}
		}
	}
}
