using Devdeb.Serialization;
using System;

namespace Devdeb.Storage.Migrators
{
	public abstract class EntityMigrator<TCurrentVersion>
	{
		protected EntityMigrator()
		{
			if (CurrentSerializer.Flags == SerializerFlags.NeedCount)
				throw new Exception($"{nameof(CurrentSerializer)} should not need a count parameter in deserialize {nameof(ISerializer<object>.Deserialize)}.");
		}

		public abstract int Version { get; }
		public abstract ISerializer<TCurrentVersion> CurrentSerializer { get; }
		public virtual TCurrentVersion Convert(int version, byte[] buffer, int offset)
		{
			if (version != Version)
				throw new ArgumentException(nameof(version));
			return CurrentSerializer.Deserialize(buffer, offset);
		}
	}
}
