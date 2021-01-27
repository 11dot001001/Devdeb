using Devdeb.Storage.Migrators;
using System;

namespace Devdeb.Storage
{
	public class Data<T>
	{
		private readonly DataSource _dataSource;
		private readonly StoredReference<StoredEvolution<T>> _instance;
		private readonly EntityMigrator<T> _entityMigrator;

		internal Data
		(
			DataSource dataSource,
			StoredReference<StoredEvolution<T>> instance,
			EntityMigrator<T> entityMigrator
		)
		{
			_dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
			_instance = instance ?? throw new ArgumentNullException(nameof(instance));
			_entityMigrator = entityMigrator ?? throw new ArgumentNullException(nameof(entityMigrator));
		}

		public bool Add(T instance)
		{
			if (Contains())
				return false;

			_instance.Value = new StoredEvolution<T>
			{
				Version = _entityMigrator.Version,
				Data = instance
			};
			_dataSource.Upload(_instance);
			return true;
		}
		public void Remove()
		{
			if (!Contains())
				return;

			_instance.Value = null;
			_dataSource.Upload(_instance);
		}
		public void Update(T instance)
		{
			_instance.Value = new StoredEvolution<T>
			{
				Version = _entityMigrator.Version,
				Data = instance
			};
			_dataSource.Upload(_instance);
		}
		public T Get() => !Contains() ? default : _instance.Value.Data;
		public bool Contains() => _instance.Value != null && _instance.Value.Data != null;
	}
}
