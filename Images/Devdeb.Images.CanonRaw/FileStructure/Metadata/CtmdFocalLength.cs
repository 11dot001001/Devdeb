using Devdeb.Serialization.Serializers.System;
using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Metadata
{
    public struct CtmdFocalLength
    {
        public CtmdFocalLength(ReadOnlyMemory<byte> memory)
        {
            FocalLengthNumerator = Int16Serializer.Default.Deserialize(memory.Slice(0, 2).ToArray(), 0);
            FocalLengthDenominator = Int16Serializer.Default.Deserialize(memory.Slice(2, 2).ToArray(), 0);
            var unknown = memory.Slice(4, 8);
        }

        public short FocalLengthNumerator { get; }
        public short FocalLengthDenominator { get; }
    }
}
