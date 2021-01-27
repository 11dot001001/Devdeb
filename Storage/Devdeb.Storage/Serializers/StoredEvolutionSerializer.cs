using Devdeb.Serialization;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Storage.Migrators;
using System;

namespace Devdeb.Storage.Serializers
{
	internal class StoredEvolutionSerializer<StoredType> : Serializer<StoredEvolution<StoredType>>
	{
		private readonly EntityMigrator<StoredType> _migrator;
		private readonly Int32Serializer _int32Serializer;

		public StoredEvolutionSerializer(EntityMigrator<StoredType> entityMigrator)
		{
			_migrator = entityMigrator ?? throw new ArgumentNullException(nameof(entityMigrator));
			_int32Serializer = new Int32Serializer();
		}

		public override int Size(StoredEvolution<StoredType> instance)
		{
			VerifySize(instance);
			return _int32Serializer.Size + _migrator.CurrentSerializer.Size(instance.Data);
		}
		public override void Serialize(StoredEvolution<StoredType> instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_int32Serializer.Serialize(instance.Version, buffer, ref offset);
			_migrator.CurrentSerializer.Serialize(instance.Data, buffer, ref offset);
		}
		public override StoredEvolution<StoredType> Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			int version = _int32Serializer.Deserialize(buffer, ref offset);
			StoredType storedValue = _migrator.Convert(version, buffer, offset);
			return new StoredEvolution<StoredType>
			{
				Version = _migrator.Version,
				Data = storedValue
			};
		}
	}
}
