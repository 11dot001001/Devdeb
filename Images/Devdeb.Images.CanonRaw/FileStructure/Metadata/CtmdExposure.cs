using Devdeb.Serialization.Serializers.System;
using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Metadata
{
    public struct CtmdExposure
    {
        public CtmdExposure(ReadOnlyMemory<byte> memory)
        {
            FNumberNumerator = Int16Serializer.Default.Deserialize(memory.Slice(0, 2).ToArray(), 0);
            FNumberDenominator = Int16Serializer.Default.Deserialize(memory.Slice(2, 2).ToArray(), 0);
            ExposureTimeNumerator = Int16Serializer.Default.Deserialize(memory.Slice(4, 2).ToArray(), 0);
            ExposureTimeDenominator = Int16Serializer.Default.Deserialize(memory.Slice(6, 2).ToArray(), 0);
            IsoSpeedRating = Int32Serializer.Default.Deserialize(memory.Slice(8, 4).ToArray(), 0);
            var unknown = memory.Slice(12, 16);
        }

        public short FNumberNumerator { get; }
        public short FNumberDenominator { get; }
        public short ExposureTimeNumerator { get; }
        public short ExposureTimeDenominator { get; }
        public int IsoSpeedRating { get; }

        public string F => $"F/{(float)FNumberNumerator / FNumberDenominator}";
    }
}
