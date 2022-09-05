using Devdeb.Serialization.Serializers.System;
using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Chunks
{
	public static class ChunkNames
	{
		public const string FileTypeBox = "ftyp";
		public static ReadOnlyMemory<byte> FileTypeBoxMemory { get; } = GetMemory(FileTypeBox);

		public const string MovieBox = "moov";
		public const string MediaDataBox = "mdat";

		static private ReadOnlyMemory<byte> GetMemory(string name)
		{
			byte[] memory = new byte[StringSerializer.Default.Size(name)];
			StringSerializer.Default.Serialize(name, memory, 0);
			return memory;
		}
	}
}
