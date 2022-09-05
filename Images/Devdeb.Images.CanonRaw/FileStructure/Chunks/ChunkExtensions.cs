using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;

namespace Devdeb.Images.CanonRaw.FileStructure.Chunks
{
    public static class ChunkExtensions
    {
        static public Chunk ReadChunk(ReadOnlyMemory<byte> memory)
        {
            uint length = UInt32Serializer.BigEndian.Deserialize(memory[..4].ToArray(), 0);

            byte[] nameMemory = memory.Slice(4, 4).ToArray();
            string name = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
            int dataOffset = 8;
            if (length == 1)
            {
                length = checked((uint)Int64Serializer.BigEndian.Deserialize(memory[8..16].ToArray(), 0));
                dataOffset += 8;
            }
            ReadOnlyMemory<byte> chunkData = memory[dataOffset..checked((int)length)];

            return new Chunk
            {
                Length = length,
                Name = name,
                Memory = chunkData
            };
        }

        static public Dictionary<string, Chunk> ReadChunks(ReadOnlyMemory<byte> memory)
        {
            Dictionary<string, Chunk> chunks = new();
            for (int i = 0; memory.Length != 0; i++)
            {
                Chunk chunk = ReadChunk(memory);
                AddChunk(chunks, chunk);
                memory = memory[(int)chunk.Length..];
            }
            return chunks;

            static void AddChunk(Dictionary<string, Chunk> chunks, Chunk chunk)
            {
                if (!chunks.TryGetValue(chunk.Name, out _))
                {
                    chunks.Add(chunk.Name, chunk);
                    return;
                }

                for (int i = 1; ; i++)
                {
                    var chunkName = $"{chunk.Name} {i}";
                    if (!chunks.TryGetValue(chunkName, out _))
                    {
                        chunks.Add(chunkName, chunk);
                        return;
                    }
                }
            }
        }
    }
}
