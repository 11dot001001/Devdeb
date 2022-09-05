using Devdeb.Serialization.Serializers.System;
using System;
using System.Linq;

namespace Devdeb.Images.CanonRaw.FileStructure.Image
{
    public class SubbandHeader
    {
        public static short[] Signatures { get; } = new short[] { unchecked((short)0xFF03), unchecked((short)0xFF13) };
        public SubbandHeader(ReadOnlyMemory<byte> memory)
        {
            var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
            if (!Signatures.Contains(signature))
                throw new InvalidOperationException($"Invalid subband header signature {signature}.");

            HdrSize = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
            SubbandDataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
            Counter = memory.Span[8] >> 4; //0
            DoesSupportsPartialFlag = (memory.Span[8] >> 3) & 1; //0
            QParam = (byte)((memory.Span[8] << 5) | (memory.Span[9] >> 3)); //4
            Unknown = (memory.Span[9] & 7 << 16) | (memory.Span[10] << 8) | (memory.Span[11]); //2  

            var bitData = Int32Serializer.BigEndian.Deserialize(memory.Slice(8, 4).ToArray(), 0);
            DataSize = SubbandDataSize - (bitData & 0x7FFFF);
        }

        public short HdrSize { get; }
        /// <remarks>Sum of plane data equals size of parent tile.</remarks>
        public int SubbandDataSize { get; }
        public int Counter { get; }
        public int DoesSupportsPartialFlag { get; }
        /// <remarks>QuantValue</remarks>
        public byte QParam { get; }
        public int Unknown { get; }
        public long DataSize { get; set; }
    }
}
