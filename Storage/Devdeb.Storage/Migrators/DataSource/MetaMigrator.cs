using Devdeb.Serialization;
using Devdeb.Storage.Serializers;

namespace Devdeb.Storage.Migrators.DataSource
{
	internal sealed class MetaMigrator : EntityMigrator<Meta>
	{
		public override int Version => 1;
		public override ISerializer<Meta> CurrentSerializer => MetaSeriaizer.Default;
	}
}
