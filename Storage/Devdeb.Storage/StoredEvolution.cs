namespace Devdeb.Storage
{
	public class StoredEvolution<StoredType>
	{
		public int Version { get; set; }
		public StoredType Data { get; set; }
	}
}
