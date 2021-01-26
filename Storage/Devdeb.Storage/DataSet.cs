using Devdeb.Sets.Generic;
using Devdeb.Sorage.SorableHeap;
using Devdeb.Storage.Migrators;
using Devdeb.Storage.Serializers;
using System;
using System.Linq;

namespace Devdeb.Storage
{
	public class DataSet<T>
	{
		private readonly StorableHeap _storableHeap;
		private readonly DataSource _dataSource;
		private readonly StoredReference<RedBlackTreeSurjection<int, Segment>> _primaryIndexes;
		private readonly EntityMigrator<T> _entityMigrator;
		private readonly StoredEvolutionSerializer<T> _storedEntitySerializer;

		internal DataSet
		(
			StorableHeap storableHeap,
			DataSource dataSource,
			StoredReference<RedBlackTreeSurjection<int, Segment>> primaryIndexes,
			EntityMigrator<T> entityMigrator
		)
		{
			_storableHeap = storableHeap ?? throw new ArgumentNullException(nameof(storableHeap));
			_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
			_primaryIndexes = primaryIndexes ?? throw new ArgumentNullException(nameof(primaryIndexes));
			_entityMigrator = entityMigrator ?? throw new ArgumentNullException(nameof(entityMigrator));
			_storedEntitySerializer = new StoredEvolutionSerializer<T>(_entityMigrator);
		}

		public bool Add(int id, T instance)
		{
			if (_primaryIndexes.Value.TryGetValue(id, out _))
				return false;

			StoredEvolution<T> storedEntity = new StoredEvolution<T>
			{
				Version = _entityMigrator.Version,
				Data = instance
			};

			int instanceLength = _storedEntitySerializer.Size(storedEntity);
			Segment instanceSegment = _storableHeap.AllocateMemory(instanceLength);
			_primaryIndexes.Value.Add(id, instanceSegment);

			byte[] buffer = new byte[instanceLength];
			_storedEntitySerializer.Serialize(storedEntity, buffer, 0);
			_storableHeap.Write(instanceSegment, buffer, 0, buffer.Length);
			_dataSource.Upload(_primaryIndexes);
			return true;
		}
		public bool TryGetById(int id, out T instance)
		{
			if (!_primaryIndexes.Value.TryGetValue(id, out Segment segment))
			{
				instance = default;
				return false;
			}

			byte[] buffer = new byte[segment.Size]; //segment.Size incredible crutch may be.
			_storableHeap.ReadBytes(segment, buffer, 0, buffer.Length);
			instance = _storedEntitySerializer.Deserialize(buffer, 0).Data;
			return true;
		}
		public void RemoveById(int id)
		{
			if (!_primaryIndexes.Value.Remove(id, out Segment segment))
				return;
			_storableHeap.FreeMemory(segment);
			_dataSource.Upload(_primaryIndexes);
		}
		public T[] GetAll()
		{
			Segment[] segments = _primaryIndexes.Value.Select(x => x.Output).ToArray();
			T[] result = new T[segments.Length];
			for (int i = 0; i != result.Length; i++)
			{
				byte[] buffer = new byte[segments[i].Size]; //segment.Size incredible crutch may be.
				_storableHeap.ReadBytes(segments[i], buffer, 0, buffer.Length);
				result[i] = _storedEntitySerializer.Deserialize(buffer, 0).Data;
			}
			return result;
		}
	}
}
