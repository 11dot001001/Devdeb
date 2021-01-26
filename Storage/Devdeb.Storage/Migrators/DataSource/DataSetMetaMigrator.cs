using Devdeb.Serialization;
using Devdeb.Storage.Serializers;

namespace Devdeb.Storage.Migrators.DataSource
{
	internal sealed class DataSetMetaMigrator : EntityMigrator<DataSetMeta>
	{
		public override int Version => 0;
		public override ISerializer<DataSetMeta> CurrentSerializer => StorageSerializers.DataSetMetaSerializer;
	}
}
