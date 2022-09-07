using Devdeb.Images.CanonRaw.Decoding;
using Devdeb.Images.CanonRaw.FileStructure;
using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using Devdeb.Images.CanonRaw.FileStructure.Image;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Devdeb.Images.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            var imageBuffer = ReturnImageBuffer();

            BitmapSource bitmapSource = BitmapSource.Create(5568, 3706, 0, 0, PixelFormats.Rgb24, BitmapPalettes.WebPalette, imageBuffer, 16704);

            Image.Source = bitmapSource;
        }

        static byte[] ReturnImageBuffer()
        {
            using var fileStream = new FileStream(@"C:\Users\lehac\Desktop\IMG_3184.CR3", FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fileStream.Length];

            fileStream.Read(buffer, 0, buffer.Length);

            var signature = ((Memory<byte>)buffer).Slice(4, 8);
            string someString = "ftypcrx ";
            byte[] someStringBuffer = new byte[StringSerializer.Default.Size(someString)];
            StringSerializer.Default.Serialize(someString, someStringBuffer, 0);
            if (signature.Span.SequenceEqual(someStringBuffer))
                return Parse(0, buffer, 0, 0);
            return null;
        }

        static byte[] Parse(int offset, Memory<byte> fileMemory, int @base, int depth)
        {
            int localOffset = 0;
            List<Chunk> chunks = new();
            for (int chunkIndex = 0; localOffset < fileMemory.Length; chunkIndex++)
            {
                var chunk = ChunkExtensions.ReadChunk(fileMemory[localOffset..]);
                chunks.Add(chunk);
                localOffset += checked((int)chunk.Length);
            }
            CannonRaw3 cannonRaw3 = new(chunks);

            return ParseCrxHdImage(cannonRaw3.MovieBox.CrxHdImageTrack, fileMemory);
        }

        static byte[] ParseCrxHdImage(TrackBox crxHdImageTrack, Memory<byte> fileMemory)
        {
            var ctmd = crxHdImageTrack.SampleTable;

            var memory = fileMemory.Slice(checked((int)ctmd.Offset.Offset), ctmd.Size.EntrySizes[0]);

            var hdr = ctmd.Craw.Compression;

            if (hdr.PlanesNumber == 4)
            {
                hdr.FWidth >>= 1;
                hdr.FHeight >>= 1;
                hdr.TileWidth >>= 1;
                hdr.TileHeight >>= 1;
            }
            var imgdata_color_maximum = (1 << hdr.BitsPerSample) - 1;

            ReadOnlyMemory<byte> tileHeaderMemory = memory;
            var tileHeader = new TileHeader(ref tileHeaderMemory);

            var tileMemory = memory[ctmd.Craw.Compression.MdatTrackHeaderSize..];

            var planeOffset = 0;
            var redMemory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[0].SubbandHeader.DataSize);
            planeOffset += tileHeader.PlaneHeaders[0].PlaneDataSize;
            var green1Memory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[1].SubbandHeader.DataSize);
            planeOffset += tileHeader.PlaneHeaders[1].PlaneDataSize;
            var green2Memory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[2].SubbandHeader.DataSize);
            planeOffset += tileHeader.PlaneHeaders[2].PlaneDataSize;
            var blueMemory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[3].SubbandHeader.DataSize);

            List<ushort[]> red = null;
            List<ushort[]> green1 = null;
            List<ushort[]> green2 = null;
            List<ushort[]> blue = null;
            Task[] planeDecode = new Task[]
            {
                Task.Run(() => red = PlaneDecoder.DecodePlane(redMemory, hdr, tileHeader, 0)),
                Task.Run(() => green1 = PlaneDecoder.DecodePlane(green1Memory, hdr, tileHeader, 1)),
                Task.Run(() => green2 = PlaneDecoder.DecodePlane(green2Memory, hdr, tileHeader, 2)),
                Task.Run(() => blue = PlaneDecoder.DecodePlane(blueMemory, hdr, tileHeader, 3))
            };
            Task.WhenAll(planeDecode).GetAwaiter().GetResult();

            var subbandWidth = red[0].Length;
            var subbandHeight = red.Count;


            var max_val = (1 << 14) - 1;

            byte[] imageBuffer = new byte[subbandHeight * 2 * subbandWidth * 2 * 3];
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.For(0, subbandHeight - 1, parallelOptions, height =>
            {
                for (int width = 0; width != subbandWidth - 1; width++)
                {
                    var redValue = ((double)red[height][width] / max_val) * 255;
                    var green1Value = ((double)green1[height][width] / max_val) * 255;
                    var green2Value = ((double)green2[height][width] / max_val) * 255;
                    var blueValue = ((double)blue[height][width] / max_val) * 255;

                    imageBuffer[GetByteIndex(width * 2, height * 2, subbandWidth * 2)] = (byte)redValue;
                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2, subbandWidth * 2) + 1] = (byte)green1Value;
                    imageBuffer[GetByteIndex(width * 2, height * 2 + 1, subbandWidth * 2) + 1] = (byte)green2Value;
                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2 + 1, subbandWidth * 2) + 2] = (byte)blueValue;
                }
            });

            return imageBuffer;
        }

        private static int GetByteIndex(int width, int height, int lineLength)
        {
            return (height * lineLength + width) * 3;
        }
    }
}
