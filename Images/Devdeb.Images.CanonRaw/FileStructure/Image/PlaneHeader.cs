using Devdeb.Serialization.Serializers.System;
using System;
using System.Linq;

namespace Devdeb.Images.CanonRaw.FileStructure.Image
{
    public class PlaneHeader
    {
        static public short[] Signatures { get; } = new short[] { unchecked((short)0xFF02), unchecked((short)0xFF12) };

        public PlaneHeader(ref ReadOnlyMemory<byte> memory)
        {
            Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
            PlaneDataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
            Counter = memory.Span[8] >> 4;
            DoesSupportsPartialFlag = (memory.Span[8] & 8) != 0;
            RoundedBits = (memory.Span[8] >> 1) & 3; //0

            var compHdrRoundedBits = (memory.Span[8] >> 1) & 3;

            memory = memory[12..];
            SubbandHeader = new SubbandHeader(memory);
            memory = memory[12..];
        }

        public short Size { get; }
        /// <remarks>Sum of plane data equals size of parent tile.</remarks>
        public int PlaneDataSize { get; }
        public int Counter { get; }
        public bool DoesSupportsPartialFlag { get; }
        public int RoundedBits { get; }
        public SubbandHeader SubbandHeader { get; }

        public static bool TryParse(ref ReadOnlyMemory<byte> memory, out PlaneHeader planeHeader)
        {
            planeHeader = default;

            var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
            if (!Signatures.Contains(signature))
                return false;

            planeHeader = new PlaneHeader(ref memory);

            return true;
        }
    }
}
