using Devdeb.Serialization.Serializers.System;
using System;

namespace Devdeb.Images.CanonRaw.FileStructure.Metadata
{
    public struct CtmdTimeStamp
    {
        public CtmdTimeStamp(ReadOnlyMemory<byte> memory)
        {
            var unknown = Int16Serializer.Default.Deserialize(memory.Slice(0, 2).ToArray(), 0);
            Year = Int16Serializer.Default.Deserialize(memory.Slice(2, 2).ToArray(), 0);
            Month = memory.Span[4];
            Day = memory.Span[5];
            Hour = memory.Span[6];
            Minute = memory.Span[7];
            Seconds = memory.Span[8];
            OneHundredthOfSecond = memory.Span[9];
            var unknownBuffer = memory.Slice(10, 2);
            Date = new(Year, Month, Day, Hour, Minute, Seconds);
        }

        public short Year { get; }
        public byte Month { get; }
        public byte Day { get; }
        public byte Hour { get; }
        public byte Minute { get; }
        public byte Seconds { get; }
        public byte OneHundredthOfSecond { get; }

        public DateTime Date { get; }

        public override string ToString() => Date.ToString();
    }
}
