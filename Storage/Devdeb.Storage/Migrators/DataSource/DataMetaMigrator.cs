using Devdeb.Serialization;
using Devdeb.Storage.Serializers;

namespace Devdeb.Storage.Migrators.DataSource
{
	internal sealed class DataMetaMigrator : EntityMigrator<DataMeta>
	{
		public override int Version => 0;
		public override ISerializer<DataMeta> CurrentSerializer => DataMetaSerializer.Default;
	}
}
