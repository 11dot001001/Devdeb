using Devdeb.Serialization.Serializers.System;
using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Metadata
{
    public struct CtmdRecord
    {
        public CtmdRecord(ReadOnlyMemory<byte> memory)
        {
            Size = Int32Serializer.Default.Deserialize(memory.Slice(0, 4).ToArray(), 0);
            Type = Int16Serializer.Default.Deserialize(memory.Slice(4, 2).ToArray(), 0);

            var byte1 = memory.Slice(6, 1); // 0 for non TIFF types, 1 for TIFF
            var byte2 = memory.Slice(7, 1); // 0 for non TIFF types, 1 for TIFF
            var one = Int16Serializer.Default.Deserialize(memory.Slice(8, 2).ToArray(), 0); //1
            var unknown = Int16Serializer.Default.Deserialize(memory.Slice(10, 2).ToArray(), 0); //unknown. value is 0 (types 1,3) or -1 (types 4,5,7,8,9)
            Memory = memory[12..Size];
        }

        public int Size { get; }
        public short Type { get; }
        public ReadOnlyMemory<byte> Memory { get; }

        public override string ToString() => $"Type = {Type}, Size = {Size}";
    }
}
