using Devdeb.Images.CanonRaw.Decoding;
using Devdeb.Images.CanonRaw.Drawing;
using Devdeb.Images.CanonRaw.FileStructure;
using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using Devdeb.Images.CanonRaw.FileStructure.Image;
using Devdeb.Images.CanonRaw.FileStructure.Metadata;
using Devdeb.Images.CanonRaw.IO;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Devdeb.Images.CanonRaw.Tests
{
    internal class Program
    {
        private const string _filePath = @"C:\Users\lehac\Desktop\IMG_3184.CR3";

        static async Task Main(string[] args)
        {
            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            Memory<byte> buffer = new byte[fileStream.Length];

            int readBytesCount = 0;
            for (; readBytesCount != fileStream.Length;)
                readBytesCount += await fileStream.ReadAsync(buffer);

            var signature = buffer.Slice(4, 8);
            string someString = "ftypcrx ";
            byte[] someStringBuffer = new byte[StringSerializer.Default.Size(someString)];
            StringSerializer.Default.Serialize(someString, someStringBuffer, 0);
            if (signature.Span.SequenceEqual(someStringBuffer))
                Parse(0, buffer, 0, 0);
        }

        static int Parse(int offset, Memory<byte> fileMemory, int @base, int depth)
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

            //ParseJpeg(cannonRaw3.MovieBox.JpegTrack, fileMemory);
            MetaParser.ParseMeta(cannonRaw3.MovieBox.MetaTrack, fileMemory);
            ParseCrxHdImage(cannonRaw3.MovieBox.CrxHdImageTrack, fileMemory);

            //byte[] nameMemory = metaMemory.ToArray();
            //var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);

            return 0;
        }

        static void ParseJpeg(TrackBox jpegTrack, Memory<byte> fileMemory)
        {
            var ctmd = jpegTrack.SampleTable;
            using FileStream fileStream = new(@"C:\Users\lehac\Desktop\IMG_1231_fullSize.jpeg", FileMode.Create, FileAccess.Write);
            fileStream.Write(fileMemory.Slice(checked((int)ctmd.Offset.Offset), ctmd.Size.EntrySizes[0]).Span);
        }

        static void ParseCrxHdImage(TrackBox crxHdImageTrack, Memory<byte> fileMemory)
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

            Bitmap bitmap = new(subbandWidth * 2, subbandHeight * 2, PixelFormat.Format24bppRgb);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(imageBuffer, 0, bmpData.Scan0, imageBuffer.Length);
            bitmap.UnlockBits(bmpData);

            //DrawImageBoundaries(bitmap, crxHdImageTrack);

            bitmap.Save($@"C:\Users\lehac\Desktop\bier.png", ImageFormat.Png);

            byte[] nameMemory = tileMemory.ToArray();
            var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
        }

        private static int GetByteIndex(int width, int height, int lineLength)
        {
            return (height * lineLength + width) * 3;
        }

        private static void DrawImageBoundaries(Bitmap bitmap, TrackBox crxHdImageTrack)
        {
            var leftOpticalBlackOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.LeftOpticalBlackOffset;
            Vector2 leftOpticalBlackPoint1 = new(leftOpticalBlackOffset.Left, leftOpticalBlackOffset.Top);
            Vector2 leftOpticalBlackPoint2 = new(leftOpticalBlackOffset.Right, leftOpticalBlackOffset.Bottom);
            bitmap.DrawRectangle(leftOpticalBlackPoint1, leftOpticalBlackPoint2, Color.Red);

            var topOpticalBlackOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.TopOpticalBlackOffset;
            Vector2 topOpticalBlackPoint1 = new(topOpticalBlackOffset.Left, topOpticalBlackOffset.Top);
            Vector2 topOpticalBlackPoint2 = new(topOpticalBlackOffset.Right, topOpticalBlackOffset.Bottom);
            bitmap.DrawRectangle(topOpticalBlackPoint1, topOpticalBlackPoint2, Color.Red);

            var cropOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.CropOffset;
            Vector2 cropOffsetPoint1 = new(cropOffset.Left, cropOffset.Top);
            Vector2 cropOffsetPoint2 = new(cropOffset.Right, cropOffset.Bottom);
            bitmap.DrawRectangle(cropOffsetPoint1, cropOffsetPoint2, Color.DarkRed);

            var activeAreaOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.ActiveAreaOffset;
            Vector2 activeAreaOffsetPoint1 = new(activeAreaOffset.Left, activeAreaOffset.Top);
            Vector2 activeAreaOffsetPoint2 = new(activeAreaOffset.Right, activeAreaOffset.Bottom);
            bitmap.DrawRectangle(activeAreaOffsetPoint1, activeAreaOffsetPoint2, Color.OrangeRed);
        }
    }
}
