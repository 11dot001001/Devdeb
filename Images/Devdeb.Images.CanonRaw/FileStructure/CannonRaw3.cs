using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Images.CanonRaw.FileStructure
{
    public class CannonRaw3
    {
        public CannonRaw3(List<Chunk> chunks)
        {
            if (chunks == null)
                throw new ArgumentNullException(nameof(chunks));

            FileTypeBox = new FileTypeBox(chunks.First(x => x.Name == ChunkNames.FileTypeBox));
            MovieBox = new MovieBox(chunks.First(x => x.Name == ChunkNames.MovieBox));
            MediaDataBox = new MediaDataBox(chunks.First(x => x.Name == ChunkNames.MediaDataBox));
        }

        public FileTypeBox FileTypeBox { get; }
        public MovieBox MovieBox { get; }
        public MediaDataBox MediaDataBox { get; }
    }
}
