using Devdeb.Images.CanonRaw.Decoding;
using Devdeb.Images.CanonRaw.Drawing;
using Devdeb.Images.CanonRaw.FileStructure;
using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using Devdeb.Images.CanonRaw.FileStructure.Image;
using Devdeb.Images.CanonRaw.FileStructure.Metadata;
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
        //IMG_5358 IMG_6876
        private const string _filePath = @"C:\Users\lehac\Desktop\IMG_5358.CR3";

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

        static void Parse(int offset, Memory<byte> fileMemory, int @base, int depth)
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
        }

        static void ParseJpeg(TrackBox jpegTrack, Memory<byte> fileMemory)
        {
            var ctmd = jpegTrack.SampleTable;
            using FileStream fileStream = new(@"C:\Users\lehac\Desktop\IMG_5358_fullSize.jpeg", FileMode.Create, FileAccess.Write);
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
                    //default color
                    //var redValue = ((double)red[height][width] / max_val) * 255;
                    //var green1Value = ((double)green1[height][width] / max_val) * 255;
                    //var green2Value = ((double)green2[height][width] / max_val) * 255;
                    //var blueValue = ((double)blue[height][width] / max_val) * 1.2 * 255;

                    // brightness borders: 47, 150 (in 8 bits colors)
                    //default color

                    var blackPoint =max_val * 30 / 256;
                    var whitePoint = max_val * 65 / 256;
                    var colorDepth = whitePoint - blackPoint;

                    uint red14Bit = red[height][width] - (uint)(blackPoint);
                    uint green114Bit = green1[height][width] - (uint)(blackPoint + colorDepth / 24);
                    uint green214Bit = green2[height][width] - (uint)(blackPoint + colorDepth / 24);
                    uint blue14Bit = blue[height][width] - (uint)(blackPoint);

                    red14Bit = Math.Max(0, red14Bit);
                    green114Bit = Math.Max(0, green114Bit);
                    green214Bit = Math.Max(0, green214Bit);
                    blue14Bit = Math.Max(0, blue14Bit);

                    red14Bit = Math.Min((uint)colorDepth, red14Bit);
                    green114Bit = Math.Min((uint)colorDepth, green114Bit);
                    green214Bit = Math.Min((uint)colorDepth, green214Bit);
                    blue14Bit = Math.Min((uint)colorDepth, blue14Bit);

                    var redValue = ((double)red14Bit / colorDepth) * 255;
                    var green1Value = ((double)green114Bit / colorDepth) * 255;
                    var green2Value = ((double)green214Bit / colorDepth) * 255;
                    var blueValue = ((double)blue14Bit / colorDepth) * 255;

                    //try sRGB linear
                    //var redColor = (double)red14Bit / colorDepth;
                    //var green1Color = (double)green114Bit / colorDepth;
                    //var green2Color = (double)green214Bit / colorDepth;
                    //var blueColor = (double)blue14Bit / colorDepth;

                    //var redColorValue = redColor * 3.2406d + green1Color * -1.5372d + blueColor * -0.4986d;
                    //if (redColorValue <= 0.0031308d)
                    //    redColorValue *= 12.92d;
                    //else
                    //    redColorValue = 1.055d * Math.Pow(redColorValue, 1 / 2.4) - 0.055;
                    //redValue = redColorValue * 255;

                    //var green1ColorValue = redColor * -0.9689d + green1Color * 1.8758d + blueColor * 0.0415d;
                    //if (green1ColorValue <= 0.0031308d)
                    //    green1ColorValue *= 12.92d;
                    //else
                    //    green1ColorValue = 1.055d * Math.Pow(green1ColorValue, 1 / 2.4) - 0.055;
                    //green1Value = green1ColorValue * 255;

                    //var green2ColorValue = redColor * -0.9689d + green2Color * 1.8758d + blueColor * 0.0415d;
                    //if (green2ColorValue <= 0.0031308d)
                    //    green2ColorValue *= 12.92d;
                    //else
                    //    green2ColorValue = 1.055d * Math.Pow(green2ColorValue, 1 / 2.4) - 0.055;
                    //green2Value = green2ColorValue * 255;

                    //var blueColorValue = redColor * 0.0557d + green2Color * -0.2040d + blueColor * 1.0570d;
                    //if (blueColorValue <= 0.0031308d)
                    //    blueColorValue *= 12.92d;
                    //else
                    //    blueColorValue = 1.055d * Math.Pow(blueColorValue, 1 / 2.4) - 0.055;
                    //blueValue = blueColorValue * 255;

                    //redValue = Math.Min(255, redValue);
                    //green1Value = Math.Min(255, green1Value);
                    //green2Value = Math.Min(255, green2Value);
                    //blueValue = Math.Min(255, blueValue);

                    //if ((redValue + green1Value + blueValue) / 3 < 128)
                    //{
                    //    redValue = Math.Min(255, redValue + 16);
                    //    green1Value = Math.Min(255, green1Value + 16);
                    //    green2Value = Math.Min(255, green2Value + 16);
                    //    blueValue = Math.Min(255, blueValue + 16);
                    //}

                    //demosaic
                    imageBuffer[GetByteIndex(width * 2, height * 2, subbandWidth * 2)] = (byte)redValue;
                    imageBuffer[GetByteIndex(width * 2, height * 2, subbandWidth * 2) + 1] = (byte)green1Value;
                    imageBuffer[GetByteIndex(width * 2, height * 2, subbandWidth * 2) + 2] = (byte)blueValue;

                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2, subbandWidth * 2)] = (byte)redValue;
                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2, subbandWidth * 2) + 1] = (byte)green1Value;
                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2, subbandWidth * 2) + 2] = (byte)blueValue;

                    imageBuffer[GetByteIndex(width * 2, height * 2 + 1, subbandWidth * 2)] = (byte)redValue;
                    imageBuffer[GetByteIndex(width * 2, height * 2 + 1, subbandWidth * 2) + 1] = (byte)green2Value;
                    imageBuffer[GetByteIndex(width * 2, height * 2 + 1, subbandWidth * 2) + 2] = (byte)blueValue;

                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2 + 1, subbandWidth * 2)] = (byte)redValue;
                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2 + 1, subbandWidth * 2) + 1] = (byte)green2Value;
                    imageBuffer[GetByteIndex(width * 2 + 1, height * 2 + 1, subbandWidth * 2) + 2] = (byte)blueValue;

                    //bier
                    //imageBuffer[GetByteIndex(width * 2, height * 2, subbandWidth * 2)] = (byte)redValue;
                    //imageBuffer[GetByteIndex(width * 2 + 1, height * 2, subbandWidth * 2) + 1] = (byte)green1Value;
                    //imageBuffer[GetByteIndex(width * 2, height * 2 + 1, subbandWidth * 2) + 1] = (byte)green2Value;
                    //imageBuffer[GetByteIndex(width * 2 + 1, height * 2 + 1, subbandWidth * 2) + 2] = (byte)blueValue;
                }
            });

            // brightness borders: 47, 150 (in 8 bits colors)

            Bitmap bitmap = new(subbandWidth * 2, subbandHeight * 2, PixelFormat.Format24bppRgb);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(imageBuffer, 0, bmpData.Scan0, imageBuffer.Length);
            bitmap.UnlockBits(bmpData);

            //DrawImageBoundaries(bitmap, crxHdImageTrack);

            (int[] redF, int[] greenF, int[] blueF, int[] brightness) = GetColorsFrequencies(imageBuffer);

            var minRed = redF.Min(x => x != 0);
            var minGreen = greenF.Min(x => x != 0);
            var minBlue = blueF.Min(x => x != 0);

            ColorHistogramDrawing.DrawColorChannels(redF, greenF, blueF, brightness);
            ColorHistogramDrawing.DrawColorChannels(bitmap, redF, greenF, blueF, brightness);
            bitmap.Save($@"C:\Users\lehac\Desktop\bier.png", ImageFormat.Png);

            byte[] nameMemory = tileMemory.ToArray();
            var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
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
