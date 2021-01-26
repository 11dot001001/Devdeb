using Devdeb.Serialization;
using Devdeb.Sets.Generic;
using Devdeb.Sorage.SorableHeap;
using Devdeb.Storage.Migrators;
using Devdeb.Storage.Migrators.DataSource;
using Devdeb.Storage.Serializers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Devdeb.Storage
{
	internal class Meta
	{
		internal class DataSetMetaMeta
		{
			public int Id { get; set; }
			public Segment DataSetMetaPointer { get; set; }
		}

		public List<DataSetMetaMeta> DataSetsMetaPointers { get; set; }
	}
	internal class DataSetMeta
	{
		public Segment PrimaryIndexesPointer { get; set; }
	}

	public abstract class DataSource
	{
		static private readonly EntityMigrator<Meta> _metaMigrator;
		static private readonly EntityMigrator<DataSetMeta> _dataSetMetaMigrator;
		static private readonly StoredEvolutionSerializer<Meta> _metaSerializer;
		static private readonly StoredEvolutionSerializer<DataSetMeta> _dataSetMetaSerializer;

		static DataSource()
		{
			_metaMigrator = new MetaMigrator();
			_dataSetMetaMigrator = new DataSetMetaMigrator();
			_metaSerializer = new StoredEvolutionSerializer<Meta>(_metaMigrator);
			_dataSetMetaSerializer = new StoredEvolutionSerializer<DataSetMeta>(_dataSetMetaMigrator);
		}

		private readonly StorableHeap _storableHeap;
		private readonly StoredReference<StoredEvolution<Meta>> _meta;

		protected unsafe DataSource(DirectoryInfo heapDirectory, long maxHeapSize)
		{
			_storableHeap = new StorableHeap(heapDirectory, maxHeapSize);
			_meta = InitializeMeta();
		}

		protected DataSet<T> InitializeDataSet<T>(int dataSetId, EntityMigrator<T> entityMigrator)
		{
			Meta.DataSetMetaMeta instance = _meta.Value.Data.DataSetsMetaPointers.FirstOrDefault(x => x.Id == dataSetId);
			bool isCreate = false;
			if (instance == null)
			{
				instance = new Meta.DataSetMetaMeta { Id = dataSetId };
				_meta.Value.Data.DataSetsMetaPointers.Add(instance);
				isCreate = true;
			}
			return InitializeDataSet(isCreate, InitializeDataSetMeta(isCreate, instance), entityMigrator);
		}

		internal StoredReference<T> Initialize<T>(T data, ISerializer<T> serializer)
		{
			int dataSize = serializer.Size(data);
			byte[] buffer = new byte[dataSize];
			serializer.Serialize(data, buffer, 0);
			Segment pointer = _storableHeap.AllocateMemory(dataSize);
			_storableHeap.Write(pointer, buffer, 0, buffer.Length);
			return new StoredReference<T>(pointer, data, serializer);
		}
		internal StoredReference<T> Load<T>(Segment pointer, ISerializer<T> serializer)
		{
			byte[] buffer = new byte[pointer.Size];
			_storableHeap.ReadBytes(pointer, buffer, 0, buffer.Length);
			T data = serializer.Deserialize(buffer, 0);
			return new StoredReference<T>(pointer, data, serializer);
		}
		internal void Upload<T>(StoredReference<T> referenceValue)
		{
			int size = referenceValue.Serializer.Size(referenceValue.Value);
			Segment uploadSegment = referenceValue.Pointer;
			bool resized = false;
			if (size != referenceValue.Pointer.Size)
			{
				Debug.Assert(referenceValue.Pointer != default);
				uploadSegment = _storableHeap.AllocateMemory(size);
				resized = true;
			}
			byte[] buffer = new byte[size];
			referenceValue.Serializer.Serialize(referenceValue.Value, buffer, 0);
			_storableHeap.Write(uploadSegment, buffer, 0, buffer.Length);
			if (resized)
				referenceValue.Pointer = uploadSegment;
		}

		private StoredReference<StoredEvolution<Meta>> InitializeMeta()
		{
			StoredReference<StoredEvolution<Meta>> meta = null;
			try
			{
				if (_storableHeap.IsInitializationFirst)
				{
					StoredEvolution<Meta> instance = new StoredEvolution<Meta>
					{
						Version = _metaMigrator.Version,
						Data = new Meta() { DataSetsMetaPointers = new List<Meta.DataSetMetaMeta>() }
					};
					meta = Initialize(instance, _metaSerializer);
					uploadMetaPointer(meta.Pointer);
					return meta;
				}

				byte[] buffer = new byte[StorageSerializers.SegmentSerializer.Size];
				_storableHeap.ReadBytes(_storableHeap.EntrySegment, buffer, 0, buffer.Length);
				Segment metaPointer = StorageSerializers.SegmentSerializer.Deserialize(buffer, 0);
				meta = Load(metaPointer, _metaSerializer);
				return meta;
			}
			finally { meta.PointerUpdated += (_, pointer) => uploadMetaPointer(pointer); }

			void uploadMetaPointer(Segment metaPointer)
			{
				byte[] buffer = new byte[StorageSerializers.SegmentSerializer.Size];
				StorageSerializers.SegmentSerializer.Serialize(metaPointer, buffer, 0);
				_storableHeap.Write(_storableHeap.EntrySegment, buffer, 0, buffer.Length);
			}
		}
		private unsafe StoredReference<StoredEvolution<DataSetMeta>> InitializeDataSetMeta(bool isCreate, Meta.DataSetMetaMeta dataSetMetaMeta)
		{
			Debug.Assert(_meta != default);
			StoredReference<StoredEvolution<DataSetMeta>> dataSetMeta = null;
			try
			{
				if (isCreate)
				{
					StoredEvolution<DataSetMeta> instance = new StoredEvolution<DataSetMeta>()
					{
						Version = _dataSetMetaMigrator.Version,
						Data = new DataSetMeta()
					};
					dataSetMeta = Initialize(instance, _dataSetMetaSerializer);
					uploadStoredClassDataSetPointer(dataSetMeta.Pointer);
					return dataSetMeta;
				}
				dataSetMeta = Load(dataSetMetaMeta.DataSetMetaPointer, _dataSetMetaSerializer);
				return dataSetMeta;
			}
			finally { dataSetMeta.PointerUpdated += (_, pointer) => uploadStoredClassDataSetPointer(pointer); }

			void uploadStoredClassDataSetPointer(Segment pointer)
			{
				dataSetMetaMeta.DataSetMetaPointer = pointer;
				Upload(_meta);
			}
		}
		private DataSet<T> InitializeDataSet<T>(bool isCreate, StoredReference<StoredEvolution<DataSetMeta>> dataSetMeta, EntityMigrator<T> entityMigrator)
		{
			Debug.Assert(dataSetMeta != default);
			StoredReference<RedBlackTreeSurjection<int, Segment>> primaryIndexes = null;

			if (isCreate)
			{
				primaryIndexes = Initialize(new RedBlackTreeSurjection<int, Segment>(), StorageSerializers.IndexesSerializer);
				uploadPrimaryIndexesPointer(primaryIndexes.Pointer);
			}
			else
				primaryIndexes = Load(dataSetMeta.Value.Data.PrimaryIndexesPointer, StorageSerializers.IndexesSerializer);

			primaryIndexes.PointerUpdated += (_, pointer) => uploadPrimaryIndexesPointer(pointer);
			return new DataSet<T>(_storableHeap, this, primaryIndexes, entityMigrator);

			void uploadPrimaryIndexesPointer(Segment pointer)
			{
				dataSetMeta.Value.Data.PrimaryIndexesPointer = pointer;
				Upload(dataSetMeta);
			}
		}
	}
}
