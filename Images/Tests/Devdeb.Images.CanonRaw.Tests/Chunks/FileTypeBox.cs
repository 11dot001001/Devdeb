using Devdeb.Serialization.Serializers.System;
using Devdeb.Serialization.Serializers.System.BigEndian;
using Devdeb.Serialization.Serializers.System.Collections;
using System;

namespace Devdeb.Images.CanonRaw.Tests.Chunks
{
	public class FileTypeBox
	{
		static private readonly BigEndianUInt32Serializer _uintSerializer;
		static private readonly ConstantStringSerializer _uintStringSerializer;
		static private readonly ArraySerializer<string> _uintStringArraySerializer;

		static FileTypeBox()
		{
			_uintSerializer = UInt32Serializer.BigEndian;
			_uintStringSerializer = new ConstantStringSerializer(StringSerializer.Default, sizeof(uint));
			_uintStringArraySerializer = new ArraySerializer<string>(_uintStringSerializer);
		}

		public FileTypeBox(Chunk chunk)
		{
			if (chunk.Name != ChunkNames.FileTypeBox)
				throw new ArgumentException($"Invalid chunkName. Expected: {ChunkNames.FileTypeBox}.");

			byte[] chunkMemory = chunk.Memory.ToArray();
			int offset = 0;

			MajorBrand = _uintStringSerializer.Deserialize(chunkMemory, ref offset);
			MinorVersion = _uintSerializer.Deserialize(chunkMemory, ref offset);
			
			var brandsCount = (chunkMemory.Length - offset) / UInt32Serializer.BigEndian.Size;
			CompatibleBrands = _uintStringArraySerializer.Deserialize(chunkMemory, ref offset, brandsCount);
		}

		/// <summary>
		/// Brand Identifier.
		/// </summary>
		public string MajorBrand { get; }
		/// <summary>
		/// Informative	integer for the	minor version of the major brand.
		/// </summary>
		public uint MinorVersion { get; }
		/// <summary>
		/// List of brands (to the end of the box).
		/// </summary>
		public string[] CompatibleBrands { get; }
	}
}
