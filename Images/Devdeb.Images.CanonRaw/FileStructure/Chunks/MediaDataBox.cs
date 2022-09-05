using Devdeb.Serialization.Serializers.System;
using Devdeb.Serialization.Serializers.System.BigEndian;
using Devdeb.Serialization.Serializers.System.Collections;
using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Chunks
{
	public class MediaDataBox
	{
		static private readonly BigEndianUInt32Serializer _uintSerializer;
		static private readonly ConstantStringSerializer _uintStringSerializer;
		static private readonly ArraySerializer<string> _uintStringArraySerializer;

		static MediaDataBox()
		{
			_uintSerializer = UInt32Serializer.BigEndian;
			_uintStringSerializer = new ConstantStringSerializer(StringSerializer.Default, sizeof(uint));
			_uintStringArraySerializer = new ArraySerializer<string>(_uintStringSerializer);
		}

		public MediaDataBox(Chunk chunk)
		{
			if (chunk.Name != ChunkNames.MediaDataBox)
				throw new ArgumentException($"Invalid chunkName. Expected: {ChunkNames.MediaDataBox}.");

			//var aChunk = ChunkExtensions.ReadChunk(chunk.Memory);
		}

	}
}
