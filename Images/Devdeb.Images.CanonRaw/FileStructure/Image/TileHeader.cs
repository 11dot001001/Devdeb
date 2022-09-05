using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Images.CanonRaw.FileStructure.Image
{
    public class TileHeader
    {
        public static short[] Signatures { get; } = new short[] { unchecked((short)0xFF01), unchecked((short)0xFF11) };
        public TileHeader(ref ReadOnlyMemory<byte> memory)
        {
            var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
            if (!Signatures.Contains(signature))
                throw new InvalidOperationException($"Invalid tile header signature {signature}.");

            Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
            FF01DataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
            Counter = memory.Span[8] >> 4;

            memory = memory[12..];
            PlaneHeaders = new List<PlaneHeader>();
            for (; PlaneHeader.TryParse(ref memory, out PlaneHeader planeHeader);)
                PlaneHeaders.Add(planeHeader);
        }

        public short Size { get; }
        public int FF01DataSize { get; }
        public int Counter { get; }

        public List<PlaneHeader> PlaneHeaders { get; }
    }
}
