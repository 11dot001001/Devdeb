using System;

namespace Devdeb.Storage.Migrators
{
	public abstract class EvolutionEntityMigrator<TCurrentVersion, TPrevious> : EntityMigrator<TCurrentVersion>
	{
		protected EvolutionEntityMigrator()
		{
			if (Version <= PreviousMigrator.Version)
				throw new Exception($"{nameof(Version)} must be greater than previous migrator version.");
		}

		public abstract EntityMigrator<TPrevious> PreviousMigrator { get; }
		public abstract TCurrentVersion Convert(TPrevious previous);
		public override TCurrentVersion Convert(int version, byte[] buffer, int offset)
		{
			if (version == Version)
				return CurrentSerializer.Deserialize(buffer, offset);
			return Convert(PreviousMigrator.Convert(version, buffer, offset));
		}
	}
}
