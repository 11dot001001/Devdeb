using Devdeb.Images.CanonRaw.Tests.Chunks;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

            var a = buffer.Slice(4, 8);
            string someString = "ftypcrx ";
            byte[] someStringBuffer = new byte[StringSerializer.Default.Size(someString)];
            StringSerializer.Default.Serialize(someString, someStringBuffer, 0);
            if (a.Span.SequenceEqual(someStringBuffer))
            {
                Parse(0, buffer, 0, 0);
            }
        }

        private const int NAMELEN = 4;
        private const int SIZELEN = 4;
        private const int UUID_LEN = 16;
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

            ParseJpeg(cannonRaw3.MovieBox.JpegTrack, fileMemory);
            ParseMeta(cannonRaw3.MovieBox.MetaTrack, fileMemory);
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

        public struct TileHeader
        {
            public short[] Signatures { get; } = new short[] { unchecked((short)0xFF01), unchecked((short)0xFF11) };
            public TileHeader(ref ReadOnlyMemory<byte> memory)
            {
                var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                if (!Signatures.Contains(signature))
                    throw new InvalidOperationException($"Invalid tile header signature {signature}.");

                Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
                FF01DataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                Counter = memory.Span[8] >> 4;

                memory = memory[12..];
                for (; PlaneHeader.TryParse(ref memory, out PlaneHeader planeHeader);)
                    PlaneHeaders.Add(planeHeader);
            }

            public short Size { get; }
            public int FF01DataSize { get; }
            public int Counter { get; }

            public List<PlaneHeader> PlaneHeaders { get; } = new();
        }
        public struct PlaneHeader
        {
            static public short[] Signatures { get; } = new short[] { unchecked((short)0xFF02), unchecked((short)0xFF12) };

            public PlaneHeader(ref ReadOnlyMemory<byte> memory)
            {
                Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
                PlaneDataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                Counter = memory.Span[8] >> 4;
                DoesSupportsPartialFlag = (memory.Span[8] >> 3) & 1;
                RoundedBits = (memory.Span[8] >> 1) & 3;

                memory = memory[12..];
                SubbandHeader = new SubbandHeader(memory);
                memory = memory[12..];
            }

            public short Size { get; }
            /// <remarks>Sum of plane data equals size of parent tile.</remarks>
            public int PlaneDataSize { get; }
            public int Counter { get; }
            public int DoesSupportsPartialFlag { get; }
            public int RoundedBits { get; }
            public SubbandHeader SubbandHeader { get; }

            public static bool TryParse(ref ReadOnlyMemory<byte> memory, out PlaneHeader planeHeader)
            {
                planeHeader = default;

                var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                if (!Signatures.Contains(signature))
                    return false;

                planeHeader = new PlaneHeader(ref memory);

                return true;
            }
        }
        public struct SubbandHeader
        {
            public short[] Signatures { get; } = new short[] { unchecked((short)0xFF03), unchecked((short)0xFF13) };
            public SubbandHeader(ReadOnlyMemory<byte> memory)
            {
                var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                if (!Signatures.Contains(signature))
                    throw new InvalidOperationException($"Invalid subband header signature {signature}.");

                Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
                SubbandDataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                Counter = memory.Span[8] >> 4;
                DoesSupportsPartialFlag = (memory.Span[8] >> 3) & 1;
                QuantValue = (byte)((memory.Span[8] << 5) | (memory.Span[9] >> 3));
                Unknown = (memory.Span[9] & 7 << 16) | (memory.Span[10] << 8) | (memory.Span[11]);
            }

            public short Size { get; }
            /// <remarks>Sum of plane data equals size of parent tile.</remarks>
            public int SubbandDataSize { get; }
            public int Counter { get; }
            public int DoesSupportsPartialFlag { get; }
            public byte QuantValue { get; }
            public int Unknown { get; }
        }
        static void ParseCrxHdImage(TrackBox crxHdImageTrack, Memory<byte> fileMemory)
        {
            var ctmd = crxHdImageTrack.SampleTable;

            var memory = fileMemory.Slice(checked((int)ctmd.Offset.Offset), ctmd.Size.EntrySizes[0]);


            ReadOnlyMemory<byte> tileHeaderMemory = memory;
            var tileHeader = new TileHeader(ref tileHeaderMemory);

            var tileMemory = memory[ctmd.Craw.Compression.MdatTrackHeaderSize..];

            var planeOffset = 0;
            var plane1Memory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[0].PlaneDataSize);
            planeOffset += tileHeader.PlaneHeaders[0].PlaneDataSize;
            var plane2Memory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[1].PlaneDataSize);
            planeOffset += tileHeader.PlaneHeaders[1].PlaneDataSize;
            var plane3Memory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[2].PlaneDataSize);
            planeOffset += tileHeader.PlaneHeaders[2].PlaneDataSize;
            var plane4Memory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[3].PlaneDataSize);

            var totalLEngth = plane1Memory.Length + plane2Memory.Length + plane3Memory.Length + plane4Memory.Length;

            //Create(plane1Memory, plane2Memory, plane3Memory, plane4Memory)
            //    .Save(@"C:\Users\lehac\Desktop\test_raw_picture_red.jpg", ImageFormat.Jpeg);

            int[] incrBitTable = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0 };

            //img->planeWidth
            var hdr = ctmd.Craw.Compression;
            var planeWidth = hdr.FWidth; //5568
            //img->planeHeight
            var planeHeight = hdr.FHeight; // 3708
            //img->levels
            var levels = hdr.ImageLevels; // 0
            //hdr->medianBits
            var medianBits = hdr.BitsPerSample;

            var tileCols = (planeWidth + hdr.TileWidth - 1) / hdr.TileWidth; // 1
            var tileRows = (planeHeight + hdr.TileHeight - 1) / hdr.TileHeight; // 1
            var subbandCount = 3 * levels + 1; // 1
            var samplePrecision = hdr.BitsPerSample + incrBitTable[4 * hdr.EncodingType + 2] + 1; //15

            var rowSize = 2 * planeWidth; // 11136

            // cfa layout = 0 and planes = 4 
            // R G
            // G B
            var width = planeWidth - hdr.TileWidth * (tileCols - 1); // 5568
            var progrDataSize = sizeof(int) * planeWidth; // 22272
            var paramLength = 2 * planeWidth + 4; // 11140
            //var nonProgrData = progrDataSize ? paramData + paramLength : 0;

            for (int i = 0; i < hdr.TileHeight; ++i)
            {
                //if (crxDecodeLine(planeComp->subBands->bandParam, planeComp->subBands->bandBuf))
                //    return -1;
                //int32_t* lineData = (int32_t*)planeComp->subBands->bandBuf;
                //crxConvertPlaneLine(img, imageRow + i, imageCol, planeNumber, lineData, tile->width);
            }

            DecodePlane(plane1Memory, hdr);

            CreateBitmap(plane1Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture.jpg", ImageFormat.Jpeg);
            CreateBitmap(plane2Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture2.jpg", ImageFormat.Jpeg);
            CreateBitmap(plane3Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture3.jpg", ImageFormat.Jpeg);
            CreateBitmap(plane4Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture4.jpg", ImageFormat.Jpeg);

            byte[] nameMemory = tileMemory.ToArray();
            var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
        }

        private static void DecodePlane(
            Memory<byte> plane,
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr
        )
        {
            var bitStream = new ReadOnlyBitStream(plane);

            int imageRow = 0;
            for (int tRow = 0; tRow < hdr.TileHeight; tRow++)
            {
                int imageCol = 0;
                for (int tCol = 0; tCol < hdr.TileWidth; tCol++)
                { 
                   // var tile = 
                }
                //imageRow += ;
            }
        }

        unsafe struct CrxBandParam
        {
            //public CrxBitstream BitStream { get; set; }
            public short SubbandWidth { get; set; }
            public short SubbandHeight { get; set; }
            public int RoundedBitsMask { get; set; }
            public int RoundedBits { get; set; }
            public short CurLine { get; set; }
            public int* LineBuf0 { get; set; }
            public int* LineBuf1 { get; set; }
            public int* LineBuf2 { get; set; }
            public int SParam { get; set; }
            public int KParam { get; set; }
            public int* ParamData { get; set; }
            public int* NonProgrData { get; set; }
            public bool SupportsPartial { get; set; }
        };

        static void crxSetupSubbandData(
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr
        )
        {
            long compDataSize = 0;
            long waveletDataOffset = 0;
            long compCoeffDataOffset = 0;
            var toSubbands = 3 * hdr.ImageLevels + 1;
            var transformWidth = 0;


            //for (int i = 0; i < tile->height; ++i)
            //{
            //    if (crxDecodeLine(planeComp->subBands->bandParam, planeComp->subBands->bandBuf))
            //        return -1;
            //    int32_t* lineData = (int32_t*)planeComp->subBands->bandBuf;
            //    crxConvertPlaneLine(img, imageRow + i, imageCol, planeNumber, lineData, tile->width);
            //}
        }


        static Bitmap Create(Memory<byte> plane1Memory, Memory<byte> green1, Memory<byte> green2, Memory<byte> blue)
        {
            Bitmap bitmap = new(5568, 3708);
            for (int width = 0; width != bitmap.Width; width++)
                for (int height = 0; height != bitmap.Height; height++)
                    bitmap.SetPixel(width, height, Color.White);

            int redOffset = 5;
            int blueOfset = 5;
            int green1Ofset = 5;
            int green2Ofset = 5;
            for (int height = 50; height != bitmap.Height; height++)
            {
                for (int width = 84; width != 5555; width++)
                {
                    if ((height & 1) == 1) // 1, 3
                        continue;
                    if ((width & 1) == 0) // 1, 3
                        continue;
                    if (redOffset >= plane1Memory.Length)
                        return bitmap;
                    bitmap.SetPixel(width, height, Color.FromArgb(plane1Memory.Span[redOffset], 0, 0));
                    redOffset++;
                }
                for (int width = 84; width != 5555; width++)
                {
                    if ((height & 1) == 0) //0, 2
                        continue;
                    if ((width & 1) == 1)
                        continue;
                    if (blueOfset >= blue.Length)
                        return bitmap;
                    bitmap.SetPixel(width, height, Color.FromArgb(0, 0, blue.Span[blueOfset]));
                    blueOfset++;
                }
                for (int width = 84; width != 5555; width++)
                {
                    if ((height & 1) == 1) //0, 2
                        continue;
                    if ((width & 1) == 1) //0, 2
                        continue;
                    if (green1Ofset >= green1.Length)
                        return bitmap;
                    bitmap.SetPixel(width, height, Color.FromArgb(0, green1.Span[green1Ofset], 0));
                    green1Ofset++;
                }
                for (int width = 84; width != 5555; width++)
                {
                    if ((height & 1) == 0) //0, 2
                        continue;
                    if ((width & 1) == 0) //0, 2
                        continue;
                    if (green2Ofset >= green2.Length)
                        return bitmap;
                    bitmap.SetPixel(width, height, Color.FromArgb(0, green2.Span[green2Ofset], 0));
                    green2Ofset++;
                }
            }
            return null;
        }

        static Bitmap CreateBitmap(Memory<byte> plane1Memory)
        {
            //5568 3708
            Bitmap bitmap = new(5568, 3708);
            for (int width = 0; width != bitmap.Width; width++)
                for (int height = 0; height != bitmap.Height; height++)
                    bitmap.SetPixel(width, height, Color.White);

            for (int width = 0; width != bitmap.Width; width++)
                for (int height = 0; height != bitmap.Height; height++)
                {
                    var index = width * height;
                    if (index >= plane1Memory.Length)
                        return bitmap;
                    bitmap.SetPixel(width, height, Color.FromArgb(plane1Memory.Span[index], 0, 0));
                }
            return null;
        }


        static void ParseMeta(TrackBox metaTrack, Memory<byte> fileMemory)
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

            public string F => $"F/{(float)FNumberNumerator / FNumberDenominator }";
        }

        private static Memory<byte> ChangeEndian(Memory<byte> buffer)
        {
            Memory<byte> result = new byte[buffer.Length];
            for (int i = 0; i < result.Length; i++)
                result.Span[i] = buffer.Span[result.Length - i - 1];
            return result;
        }
    }

    public struct BufferPointer
    {
        private int _bitOffset = 0;

        public int ByteIndex => _bitOffset / 8;
        public int BitIndex => _bitOffset % 8;
        public int BitOffset => _bitOffset;

        public void AddBits(int count)
        {
            checked { _bitOffset += count; }
        }
        public void SetOffset(int offset) => _bitOffset = offset;
    }
    public static class BufferPointerExtensions
    {
        public static bool DoesExceedByteThreshold(this BufferPointer bufferPointer, int threshold)
        {
            return bufferPointer.ByteIndex > threshold;
        }
        public static bool DoesExceedThreshold(this BufferPointer bufferPointer, Memory<byte> buffer)
        {
            return bufferPointer.DoesExceedByteThreshold(buffer.Length);
        }
    }
    public class ReadOnlyBitStream
    {
        private static readonly int[] _masks = new int[]
        {
            0b0000_0001,
            0b0000_0011,
            0b0000_0111,
            0b0000_1111,
            0b0001_1111,
            0b0011_1111,
            0b0111_1111,
            0b1111_1111
        };

        private readonly Memory<byte> _buffer;
        private BufferPointer _pointer;

        public ReadOnlyBitStream(Memory<byte> buffer)
        {
            _buffer = buffer;
            _pointer = new BufferPointer();
        }

        public bool Read(int bitsCount, out int value)
        {
            value = default;

            var newPointer = _pointer;
            newPointer.AddBits(bitsCount);
            if (newPointer.DoesExceedThreshold(_buffer))
                return false;

            int readCount;
            for (; bitsCount != 0; bitsCount -= readCount, _pointer.AddBits(readCount))
            {
                int byteRemainder = 8 - _pointer.BitIndex;
                readCount = Math.Min(byteRemainder, bitsCount);
                value <<= readCount;
                var byteRemaindedValue = _buffer.Span[_pointer.ByteIndex] & _masks[byteRemainder - 1];
                value |= byteRemaindedValue >> (byteRemainder - readCount);
            }

            return true;
        }
        public bool Seek(int bitsCount, SeekOrigin seekOrigin = SeekOrigin.Current)
        {
            switch (seekOrigin)
            {
                case SeekOrigin.Begin:
                    {
                        _pointer.SetOffset(bitsCount);
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        _pointer.AddBits(bitsCount);
                        break;
                    }
                case SeekOrigin.End:
                    {
                        _pointer.SetOffset(_buffer.Length - bitsCount);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(seekOrigin));
            }

            return !_pointer.DoesExceedThreshold(_buffer);
        }
    }

    public static class ChunkExtensions
    {
        static public Chunk ReadChunk(ReadOnlyMemory<byte> memory)
        {
            uint length = UInt32Serializer.BigEndian.Deserialize(memory[..4].ToArray(), 0);

            byte[] nameMemory = memory.Slice(4, 4).ToArray();
            string name = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
            int dataOffset = 8;
            if (length == 1)
            {
                length = checked((uint)Int64Serializer.BigEndian.Deserialize(memory[8..16].ToArray(), 0));
                dataOffset += 8;
            }
            ReadOnlyMemory<byte> chunkData = memory[dataOffset..checked((int)length)];

            return new Chunk
            {
                Length = length,
                Name = name,
                Memory = chunkData
            };
        }

        static public Dictionary<string, Chunk> ReadChunks(ReadOnlyMemory<byte> memory)
        {
            Dictionary<string, Chunk> chunks = new();
            for (int i = 0; memory.Length != 0; i++)
            {
                Chunk chunk = ReadChunk(memory);
                AddChunk(chunks, chunk);
                memory = memory[(int)chunk.Length..];
            }
            return chunks;

            static void AddChunk(Dictionary<string, Chunk> chunks, Chunk chunk)
            {
                if (!chunks.TryGetValue(chunk.Name, out _))
                {
                    chunks.Add(chunk.Name, chunk);
                    return;
                }

                for (int i = 1; ; i++)
                {
                    var chunkName = $"{chunk.Name} {i}";
                    if (!chunks.TryGetValue(chunkName, out _))
                    {
                        chunks.Add(chunkName, chunk);
                        return;
                    }
                }
            }
        }
    }

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
