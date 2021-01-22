using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
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
			DataSet<StoredClass> storedClassSet = dataSource.StoredClassSet;

			StoredClass[] startStoredClasses = storedClassSet.GetAll();

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
			storedClassSet.Add(storedClass1.Id, storedClass1);
			storedClassSet.Add(storedClass2.Id, storedClass2);
			storedClassSet.Add(storedClass3.Id, storedClass3);
			storedClassSet.RemoveById(15);
			bool is0WasFound = storedClassSet.TryGetById(0, out StoredClass storedClass0);
			bool is15WasFound = storedClassSet.TryGetById(15, out StoredClass storedClass15);
			bool is435234523WasFound = storedClassSet.TryGetById(435234523, out StoredClass storedClass435234523);

			StoredClass[] storedClasses = storedClassSet.GetAll();
		}

		public class StoredClass
		{
			public int Id { get; set; }
			public string Value { get; set; }
		}
		public class DataSetMeta
		{
			public Segment PrimaryIndexesPointer { get; set; }
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

		public class DataSource
		{
			private readonly StorableHeap _storableHeap;
			private readonly Meta _meta;
			private readonly DataSet<StoredClass> _storedClassSet;

			public DataSource(DirectoryInfo heapDirectory, long maxHeapSize)
			{
				_storableHeap = new StorableHeap(heapDirectory, maxHeapSize);
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
					new RedBlackTreeSurjection<int, Segment>(),
					Serializers.StoredClassSerializer
				);
				DataSet<StoredClass>.UploadDataSet(storedClassSet);
				return storedClassSet;
			}
			private DataSet<StoredClass> LoadStoredClassSet()
			{
				Debug.Assert(_meta.StoredClassSetMeta != default);
				RedBlackTreeSurjection<int, Segment> primaryIndexes = DataSet<StoredClass>.LoadPrimaryIndexes(_storableHeap, _meta.StoredClassSetMeta);
				return new DataSet<StoredClass>(_storableHeap, this, _meta.StoredClassSetMeta, primaryIndexes, Serializers.StoredClassSerializer);
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
				return Serializers.MetaSeriaizer.Deserialize(buffer, 0);
			}
			private unsafe void UploadMeta(Meta meta)
			{
				int metaLength = Serializers.MetaSeriaizer.Size;
				Segment metaSegment = LoadMetaSegment();
				if (metaLength != metaSegment.Size)
				{
					if (metaSegment != default)
						_storableHeap.FreeMemory(metaSegment);
					metaSegment = _storableHeap.AllocateMemory(metaLength);
					UploadMetaSegment(metaSegment);
				}

				byte[] buffer = new byte[metaLength];
				Serializers.MetaSeriaizer.Serialize(meta, buffer, 0);
				_storableHeap.Write(metaSegment, buffer, 0, buffer.Length);
			}
			internal void UploadMeta() => UploadMeta(_meta);

			private Segment InitializaionSegment => new Segment
			{
				Pointer = 0,
				Size = Serializers.SegmentSerializer.Size
			};
			private void InitializeMetaSegment()
			{
				Debug.Assert(_storableHeap.AllocateMemory(Serializers.SegmentSerializer.Size) == InitializaionSegment);
				UploadMetaSegment(default);
			}
			private Segment LoadMetaSegment()
			{
				byte[] buffer = new byte[Serializers.SegmentSerializer.Size];
				_storableHeap.ReadBytes(InitializaionSegment, buffer, 0, buffer.Length);
				return Serializers.SegmentSerializer.Deserialize(buffer, 0);
			}
			private void UploadMetaSegment(Segment segment)
			{
				byte[] buffer = new byte[Serializers.SegmentSerializer.Size];
				Serializers.SegmentSerializer.Serialize(segment, buffer, 0);
				_storableHeap.Write(InitializaionSegment, buffer, 0, buffer.Length);
			}
		}
		public class DataSet<T>
		{
			static internal RedBlackTreeSurjection<int, Segment> LoadPrimaryIndexes(StorableHeap storableHeap, DataSetMeta meta)
			{
				Debug.Assert(meta.PrimaryIndexesPointer != default);
				byte[] buffer = new byte[meta.PrimaryIndexesPointer.Size];
				storableHeap.ReadBytes(meta.PrimaryIndexesPointer, buffer, 0, buffer.Length);
				return new RedBlackTreeSurjection<int, Segment>(Serializers.IndexesSerializer.Deserialize(buffer, 0));
			}
			static internal void UploadDataSet(DataSet<T> dataSet) => UploadPrimaryIndexes(dataSet);

			static private void UploadPrimaryIndexes(DataSet<T> dataSet)
			{
				SurjectionRatio<int, Segment>[] indexes = dataSet._primaryIndexes.ToArray();
				int size = Serializers.IndexesSerializer.Size(indexes);
				if (size != dataSet._meta.PrimaryIndexesPointer.Size)
				{
					if (dataSet._meta.PrimaryIndexesPointer != default)
						dataSet._storableHeap.FreeMemory(dataSet._meta.PrimaryIndexesPointer);
					dataSet._meta.PrimaryIndexesPointer = dataSet._storableHeap.AllocateMemory(size);
					dataSet._dataSource.UploadMeta();
				}
				byte[] buffer = new byte[size];
				Serializers.IndexesSerializer.Serialize(indexes, buffer, 0);
				dataSet._storableHeap.Write(dataSet._meta.PrimaryIndexesPointer, buffer, 0, buffer.Length);
			}

			private readonly StorableHeap _storableHeap;
			private readonly DataSource _dataSource;
			private readonly DataSetMeta _meta;
			private readonly RedBlackTreeSurjection<int, Segment> _primaryIndexes;
			private readonly ISerializer<T> _entitySerializer;

			public DataSet
			(
				StorableHeap storableHeap,
				DataSource dataSource,
				DataSetMeta dataSetMeta,
				RedBlackTreeSurjection<int, Segment> primaryIndexes,
				ISerializer<T> entitySerializer
			)
			{
				_storableHeap = storableHeap ?? throw new ArgumentNullException(nameof(storableHeap));
				_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
				_meta = dataSetMeta ?? throw new ArgumentNullException(nameof(dataSetMeta));
				_primaryIndexes = primaryIndexes ?? throw new ArgumentNullException(nameof(primaryIndexes));
				_entitySerializer = entitySerializer ?? throw new ArgumentNullException(nameof(entitySerializer));
			}

			public bool Add(int id, T instance)
			{
				if (_primaryIndexes.TryGetValue(id, out _))
					return false;

				int instanceLength = _entitySerializer.Size(instance);
				Segment instanceSegment = _storableHeap.AllocateMemory(instanceLength);
				_primaryIndexes.Add(id, instanceSegment);

				byte[] buffer = new byte[instanceLength];
				_entitySerializer.Serialize(instance, buffer, 0);
				_storableHeap.Write(instanceSegment, buffer, 0, buffer.Length);
				UploadPrimaryIndexes(this);
				return true;
			}
			public bool TryGetById(int id, out T instance)
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
			public T[] GetAll()
			{
				Segment[] segments = _primaryIndexes.Select(x => x.Output).ToArray();
				T[] result = new T[segments.Length];
				for (int i = 0; i != result.Length; i++)
				{
					byte[] buffer = new byte[segments[i].Size]; //segment.Size incredible crutch may be.
					_storableHeap.ReadBytes(segments[i], buffer, 0, buffer.Length);
					result[i] = _entitySerializer.Deserialize(buffer, 0);
				}
				return result;
			}
		}

		static internal class Serializers
		{
			static Serializers()
			{
				Int32Serializer = new Int32Serializer();
				SegmentSerializer = new SegmentSerializer();
				IndexSerializer = new SurjectionRatioSerializer<int, Segment>
				(
					new Int32Serializer(),
					SegmentSerializer
				);
				IndexesSerializer = new ArrayLengthSerializer<SurjectionRatio<int, Segment>>
				(
					IndexSerializer
				);
				StoredClassSerializer = new StoredClassSerializer();
				DataSetMetaSerializer = new DataSetMetaSerializer();
				MetaSeriaizer = new MetaSeriaizer();
			}

			static public Int32Serializer Int32Serializer { get; }
			static public SegmentSerializer SegmentSerializer { get; }
			static public SurjectionRatioSerializer<int, Segment> IndexSerializer { get; }
			static public ArrayLengthSerializer<SurjectionRatio<int, Segment>> IndexesSerializer { get; }
			static public StoredClassSerializer StoredClassSerializer { get; }
			static public DataSetMetaSerializer DataSetMetaSerializer { get; }
			static public MetaSeriaizer MetaSeriaizer { get; }
		}

		public class StoredClassSerializer : Serializer<StoredClass>
		{
			private readonly Int32Serializer _int32Serializer;
			private readonly StringLengthSerializer _stringLengthSerializer;

			public StoredClassSerializer()
			{
				_int32Serializer = new Int32Serializer();
				_stringLengthSerializer = new StringLengthSerializer(Encoding.Default);
			}

			public override int Size(StoredClass instance)
			{
				VerifySize(instance);
				return _int32Serializer.Size + _stringLengthSerializer.Size(instance.Value);
			}
			public override void Serialize(StoredClass instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				_int32Serializer.Serialize(instance.Id, buffer, ref offset);
				_stringLengthSerializer.Serialize(instance.Value, buffer, offset);
			}
			public override StoredClass Deserialize(byte[] buffer, int offset, int? count = null)
			{
				VerifyDeserialize(buffer, offset, count);
				int id = _int32Serializer.Deserialize(buffer, ref offset);
				string value = _stringLengthSerializer.Deserialize(buffer, offset);
				return new StoredClass
				{
					Id = id,
					Value = value
				};
			}
		}
		public class DataSetMetaSerializer : ConstantLengthSerializer<DataSetMeta>
		{
			public DataSetMetaSerializer() : base(Serializers.SegmentSerializer.Size) { }

			public override void Serialize(DataSetMeta instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Serializers.SegmentSerializer.Serialize(instance.PrimaryIndexesPointer, buffer, offset);
			}
			public override DataSetMeta Deserialize(byte[] buffer, int offset)
			{
				VerifyDeserialize(buffer, offset);
				Segment indexesPointer = Serializers.SegmentSerializer.Deserialize(buffer, offset);
				return new DataSetMeta() { PrimaryIndexesPointer = indexesPointer };
			}
		}
		public class MetaSeriaizer : ConstantLengthSerializer<Meta>
		{
			public MetaSeriaizer() : base(Serializers.DataSetMetaSerializer.Size) { }

			public override void Serialize(Meta instance, byte[] buffer, int offset)
			{
				VerifySerialize(instance, buffer, offset);
				Serializers.DataSetMetaSerializer.Serialize(instance.StoredClassSetMeta, buffer, offset);
			}
			public override Meta Deserialize(byte[] buffer, int offset)
			{
				VerifyDeserialize(buffer, offset);
				DataSetMeta storedClassSetMeta = Serializers.DataSetMetaSerializer.Deserialize(buffer, offset);
				return new Meta(storedClassSetMeta);
			}
		}
	}
}
