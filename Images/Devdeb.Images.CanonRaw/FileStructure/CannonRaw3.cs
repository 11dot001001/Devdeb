using Devdeb.Images.CanonRaw.Drawing;
using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using Devdeb.Images.CanonRaw.FileStructure.Image;
using Devdeb.Images.CanonRaw.FileStructure.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Devdeb.Images.CanonRaw.Decoding;

namespace Devdeb.Images.CanonRaw.FileStructure
{
    public class CannonRaw3
    {
        private readonly Memory<byte> _fileMemory;
        private readonly List<Chunk> _chunks = new();

        public CannonRaw3(Memory<byte> fileMemory)
        {
            _fileMemory = fileMemory;
            ParseChunks();

            FileTypeBox = new FileTypeBox(_chunks.First(x => x.Name == ChunkNames.FileTypeBox));
            MovieBox = new MovieBox(_chunks.First(x => x.Name == ChunkNames.MovieBox));
            MediaDataBox = new MediaDataBox(_chunks.First(x => x.Name == ChunkNames.MediaDataBox));
        }

        private void ParseChunks()
        {
            for (int localOffset = 0; localOffset < _fileMemory.Length;)
            {
                var chunk = ChunkExtensions.ReadChunk(_fileMemory[localOffset..]);
                _chunks.Add(chunk);
                localOffset += checked((int)chunk.Length);
            }
        }

        public FileTypeBox FileTypeBox { get; }
        public MovieBox MovieBox { get; }
        public MediaDataBox MediaDataBox { get; }

        public int MaxColorValue => (1 << MovieBox.CrxHdImageTrack.SampleTable.Craw.Compression.BitsPerSample) - 1;

        public int SubbandWidth {get; private set;}
        public int SubbandHeight { get; private set; }

        public AreaSize SubbandAreaSize => new()
        {
            Height = SubbandHeight,
            Width = SubbandWidth
        };

        public AreaSize ImageAreaSize => new()
        {
            Height = SubbandHeight * 2,
            Width = SubbandWidth * 2,
        };

        public void ParseFullJpeg()
        {
            var ctmd = MovieBox.JpegTrack.SampleTable;
            using FileStream fileStream = new(@"C:\Users\lehac\Desktop\IMG_5358_fullSize.jpeg", FileMode.Create, FileAccess.Write);
            fileStream.Write(_fileMemory.Slice(checked((int)ctmd.Offset.Offset), ctmd.Size.EntrySizes[0]).Span);
        }

        public void ParseMeta() => MetaParser.ParseMeta(MovieBox.MetaTrack, _fileMemory);

        public Pixel42[,] ParseCrxHdImage()
        {
            var ctmd = MovieBox.CrxHdImageTrack.SampleTable;

            var memory = _fileMemory.Slice(checked((int)ctmd.Offset.Offset), ctmd.Size.EntrySizes[0]);

            var hdr = ctmd.Craw.Compression;

            if (hdr.PlanesNumber == 4)
            {
                hdr.FWidth >>= 1;
                hdr.FHeight >>= 1;
                hdr.TileWidth >>= 1;
                hdr.TileHeight >>= 1;
            }

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

            SubbandWidth = red[0].Length;
            SubbandHeight = red.Count;

            var maxColorValue = (1 << hdr.BitsPerSample) - 1;

            Pixel42[,] pixels = new Pixel42[SubbandHeight * 2, SubbandWidth * 2];
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.For(0, SubbandHeight - 1, parallelOptions, height =>
            {
                for (int width = 0; width != SubbandWidth - 1; width++)
                {
                    ushort redValue = red[height][width];
                    ushort green1Value = green1[height][width];
                    ushort green2Value = green2[height][width];
                    ushort blueValue = blue[height][width];

                    //demosaic
                    pixels[height * 2, width * 2] = new Pixel42
                    {
                        Red = redValue,
                        Green = green1Value,
                        Blue = blueValue
                    };

                    pixels[height * 2, width * 2 + 1] = new Pixel42
                    {
                        Red = redValue,
                        Green = green1Value,
                        Blue = blueValue
                    };

                    pixels[height * 2 + 1, width * 2] = new Pixel42
                    {
                        Red = redValue,
                        Green = green2Value,
                        Blue = blueValue
                    };

                    pixels[height * 2 + 1, width * 2 + 1] = new Pixel42
                    {
                        Red = redValue,
                        Green = green2Value,
                        Blue = blueValue
                    };

                    //bier
                    //imageBuffer[GetByteIndex(width * 2, height * 2, subbandWidth * 2)] = (byte)redValue;
                    //imageBuffer[GetByteIndex(width * 2 + 1, height * 2, subbandWidth * 2) + 1] = (byte)green1Value;
                    //imageBuffer[GetByteIndex(width * 2, height * 2 + 1, subbandWidth * 2) + 1] = (byte)green2Value;
                    //imageBuffer[GetByteIndex(width * 2 + 1, height * 2 + 1, subbandWidth * 2) + 2] = (byte)blueValue;
                }
            });

            return pixels;

            // brightness borders: 47, 150 (in 8 bits colors)

            //Bitmap bitmap = new(subbandWidth * 2, subbandHeight * 2, PixelFormat.Format24bppRgb);
            //Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            //BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //Marshal.Copy(imageBuffer, 0, bmpData.Scan0, imageBuffer.Length);
            //bitmap.UnlockBits(bmpData);

            //TrashExtensions.DrawImageBoundaries(bitmap, crxHdImageTrack);

            //(int[] redF, int[] greenF, int[] blueF, int[] brightness) = GetColorsFrequencies(imageBuffer);

            //var minRed = redF.Min(x => x != 0);
            //var minGreen = greenF.Min(x => x != 0);
            //var minBlue = blueF.Min(x => x != 0);

            //ColorHistogramDrawing.DrawColorChannels(redF, greenF, blueF, brightness);
            //ColorHistogramDrawing.DrawColorChannels(bitmap, redF, greenF, blueF, brightness);
            //bitmap.Save($@"C:\Users\lehac\Desktop\bier.png", ImageFormat.Png);

            //byte[] nameMemory = tileMemory.ToArray();
            //var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
        }

        private static (int[] red, int[] green, int[] blue, int[] brightness) GetColorsFrequencies(byte[] imageData)
        {
            int[] red = new int[256];
            int[] green = new int[256];
            int[] blue = new int[256];
            int[] brightness = new int[256];

            for (int i = 0; i < imageData.Length / 3; i += 3)
            {
                var redValue = imageData[i];
                var greenValue = imageData[i + 1];
                var blueValue = imageData[i + 2];
                red[redValue]++;
                green[greenValue]++;
                blue[blueValue]++;
                brightness[(redValue + greenValue + blueValue) / 3]++;
            }
            return (red, green, blue, brightness);
        }

        private static int GetByteIndex(int width, int height, int lineLength) => (height * lineLength + width) * 3;
    }
}
