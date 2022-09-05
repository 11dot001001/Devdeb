using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Chunks
{
    public struct Chunk
	{
		public uint Length { get; init; }
		public string Name { get; init; }
		public ReadOnlyMemory<byte> Memory { get; init; }

		public override string ToString() => $"{Name} {{{Length}}}";
	}
}
