using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Images.CanonRaw.FileStructure.Metadata
{
    public class MetaParser
    {
        static public void ParseMeta(TrackBox metaTrack, Memory<byte> fileMemory)
        {
            var ctmd = metaTrack.SampleTable;
            var metaMemory = fileMemory.Slice(checked((int)ctmd.Offset.Offset), ctmd.Size.SampleSize);

            List<CtmdRecord> records = new();
            for (; metaMemory.Length != 0;)
            {
                CtmdRecord record = new(metaMemory);
                records.Add(record);
                metaMemory = metaMemory[record.Size..];
            }
            CtmdTimeStamp timeStamp = new(records.First(x => x.Type == 1).Memory);
            CtmdFocalLength focalLength = new(records.First(x => x.Type == 4).Memory);
            CtmdExposure exposure = new(records.First(x => x.Type == 5).Memory);
            // add 7,8,9 in tiff format
        }
    }
}
