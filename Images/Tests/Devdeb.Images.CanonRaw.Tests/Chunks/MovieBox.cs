using Devdeb.Serialization.Serializers.System.BigEndian;
using Devdeb.Serialization.Serializers.System.Collections;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Devdeb.Images.CanonRaw.Tests.Chunks
{
    public class MovieBox
    {
        static private readonly BigEndianUInt32Serializer _uintSerializer;
        static private readonly ConstantStringSerializer _uintStringSerializer;
        static private readonly ArraySerializer<string> _uintStringArraySerializer;

        static MovieBox()
        {
            _uintSerializer = UInt32Serializer.BigEndian;
            _uintStringSerializer = new ConstantStringSerializer(StringSerializer.Default, sizeof(uint));
            _uintStringArraySerializer = new ArraySerializer<string>(_uintStringSerializer);
        }

        public MovieBox(Chunk chunk)
        {
            if (chunk.Name != ChunkNames.MovieBox)
                throw new ArgumentException($"Invalid chunkName. Expected: {ChunkNames.MovieBox}.");

            var chunks = ChunkExtensions.ReadChunks(chunk.Memory);

            var uuidChunk = chunks["uuid"];

            #region uuid = 85c0b687 820f 11e0 8111 f4ce462b6a48
            var userType = uuidChunk.Memory.Slice(0, 16);

            uint tagSize = UInt32Serializer.BigEndian.Deserialize(uuidChunk.Memory.Slice(16, 4).ToArray(), 0);
            byte[] nameMemory = uuidChunk.Memory.Slice(20, 4).ToArray();
            string name = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length); // CNCV - Canon Compressor Version

            byte[] versionStringMemory = uuidChunk.Memory.Slice(16 + 8, 30).ToArray();
            string versionString = StringSerializer.Default.Deserialize(versionStringMemory, 0, versionStringMemory.Length);

            var leastMemory = uuidChunk.Memory[54..];
            var uuidChunks = ChunkExtensions.ReadChunks(leastMemory);

            // CCTP
            var cctpChunk = uuidChunks["CCTP"]; // CCTP - Canon Compressor Table Pointers
            var cctpChunks = ChunkExtensions.ReadChunks(cctpChunk.Memory[12..]);
            var ccdt1Chunk = cctpChunks["CCDT"]; // CCDT 1
            var ccdt2Chunk = cctpChunks["CCDT 1"]; // CCDT 2
            var ccdt3Chunk = cctpChunks["CCDT 2"]; // CCDT 3

            var ctboChunk = uuidChunks["CTBO"]; // CTBO - Canon Trak b Offsets

            uint ctboTagSize = UInt32Serializer.BigEndian.Deserialize(ctboChunk.Memory.Slice(0, 4).ToArray(), 0);

            // .. 5 
            CtboRecord xpacket = new(ctboChunk.Memory.Slice(4, 20));
            CtboRecord preview = new(ctboChunk.Memory.Slice(24, 20));
            CtboRecord mdat = new(ctboChunk.Memory.Slice(44, 20));
            CtboRecord zero = new(ctboChunk.Memory.Slice(64, 20));
            CtboRecord uuidCMTA = new(ctboChunk.Memory.Slice(84, 20));

            var cmt1Chunk = uuidChunks["CMT1"]; // CMT1
            var cmt2Chunk = uuidChunks["CMT2"]; // CMT2
            var cmt3Chunk = uuidChunks["CMT3"]; // CMT3
            var cmt4Chunk = uuidChunks["CMT4"]; // CMT4
            var thmbChunk = uuidChunks["THMB"]; // THMB

            var thmbVersion = thmbChunk.Memory.Span[0];
            var thmbFlags = thmbChunk.Memory.Slice(1, 3);

            if (thmbVersion == 0)
            {
                var width = Int16Serializer.BigEndian.Deserialize(thmbChunk.Memory.Slice(4, 2).ToArray(), 0);
                var height = Int16Serializer.BigEndian.Deserialize(thmbChunk.Memory.Slice(6, 2).ToArray(), 0);
                var jpegSize = Int32Serializer.BigEndian.Deserialize(thmbChunk.Memory.Slice(8, 4).ToArray(), 0);
                var unknown1 = Int16Serializer.BigEndian.Deserialize(thmbChunk.Memory.Slice(12, 2).ToArray(), 0);
                var unknown2 = Int16Serializer.BigEndian.Deserialize(thmbChunk.Memory.Slice(14, 2).ToArray(), 0);
                var thmbLeastMemory = thmbChunk.Memory[16..];
                Bitmap jpeg = new(new MemoryStream(thmbLeastMemory.Span.ToArray()));
                jpeg.RotateFlip(RotateFlipType.Rotate270FlipNone);
                jpeg.Save(@"C:\Users\lehac\Desktop\IMG_12312.jpeg", ImageFormat.Jpeg);
                using FileStream fileStream = new(@"C:\Users\lehac\Desktop\IMG_1231.jpeg", FileMode.Create, FileAccess.Write);
                fileStream.Write(thmbLeastMemory.Span);
            }
            #endregion

            var mvhdChunk = chunks["mvhd"]; //Movie Header

            JpegTrack = new TrackBox(chunks["trak"]);
            SdCrxTrack = new TrackBox(chunks["trak 1"]);
            CrxHdImageTrack = new TrackBox(chunks["trak 2"]);
            MetaTrack = new TrackBox(chunks["trak 3"]);
        }

        public TrackBox JpegTrack { get; }
        public TrackBox SdCrxTrack { get; }
        public TrackBox CrxHdImageTrack { get; }
        public TrackBox MetaTrack { get; }
    }

    public class TrackBox
    {
        /// <remarks>vmhd</remarks>
        public class VideoMediaHeaderBox
        {
            public VideoMediaHeaderBox(ReadOnlyMemory<byte> memory)
            {

            }
        }
        /// <remarks>dinf</remarks>
        public class DataInformationBox
        {
            public DataInformationBox(ReadOnlyMemory<byte> memory)
            {
                var drefChunk = ChunkExtensions.ReadChunk(memory); //dref - data reference box


                byte[] nameMemory = drefChunk.Memory.ToArray();
                var a = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
            }
        }
        /// <remarks>stbl</remarks>
        public class SampleTableBox
        {
            public class CrawChunk
            {
                public enum ContainerType
                {
                    Jpeg = 0,
                    CrawOrRaw = 1
                }

                /// <remarks>CMP1</remarks>
                public class CompressionTag
                {
                    public CompressionTag(ReadOnlyMemory<byte> memory)
                    {
                        var unknown = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);

                        // 0x30 -size of the image header to follow
                        var imageHeaderSize = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);

                        // always 0x100 for current CR3 (major.minor version in bytes?)
                        var version = Int16Serializer.BigEndian.Deserialize(memory.Slice(4, 2).ToArray(), 0);

                        var zeroBytes = memory.Slice(6, 2);

                        FWidth = Int32Serializer.BigEndian.Deserialize(memory.Slice(8, 4).ToArray(), 0);
                        FHeight = Int32Serializer.BigEndian.Deserialize(memory.Slice(12, 4).ToArray(), 0);


                        TileWidth = Int32Serializer.BigEndian.Deserialize(memory.Slice(16, 4).ToArray(), 0); // image width /2 for big picture
                        TileHeight = Int32Serializer.BigEndian.Deserialize(memory.Slice(20, 4).ToArray(), 0);

                        BitsPerSample = memory.Span[24]; // usually 14

                        PlanesNumber = memory.Span[25] >> 4; // 4 for RGGB

                        // only valid where number of planes > 1. 0:RGGB, 1:GRBG, 2:GBRG, 3:BGGR. Seen 1 for small, 0 for big (raw or craw)
                        CfaLayout = memory.Span[25] & 0xF;

                        EncodingType = memory.Span[26] >> 4; // Always 0 for raw and craw, 3 for raw extracted from roll burst
                        ImageLevels = memory.Span[26] & 0xF; // (set for wavelet compressed image). 0 for raw, 3 for craw

                        // 1 = image has more than one tile horizontally (set for wavelet compressed image). Seen 1 for craw big, 0 otherwise
                        HasTileCols = memory.Span[27] >> 7;

                        // 1 = image has more than one tile vertically (set for wavelet compressed image). Always 0
                        HasTileRows = (memory.Span[27] >> 6) & 1;

                        // unused in current version - always 0
                        var unused = memory.Span[27] & 0x3F;

                        // mdat bitstream data starts following that header.
                        // raw small = 0x70, raw big = 0xd8, craw small = 0x220, craw big = 0x438
                        MdatTrackHeaderSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(28, 4).ToArray(), 0);

                        // 1 = has extended header, has extended header size
                        HasExtendedHeaderSize = memory.Span[32] >> 7 == 1;

                        var unknown2 = memory.Span[32] & 0xEF;
                        var unknown3 = memory.Span[33];
                        var unknown4 = Int16Serializer.BigEndian.Deserialize(memory.Slice(34, 2).ToArray(), 0);

                        // plane count (4) times "01 01 00 00"
                        var planeCount = memory.Slice(36, 16);

                        Debug.Assert(memory[52..].Length == 0);
                        // github has more data bettwen (60 .. 92) - header chunk 8 bytes
                    }
                    public int FWidth { get; }
                    public int FHeight { get; }
                    public int TileWidth { get; }
                    public int TileHeight { get; }
                    /// <remarks>nBits</remarks>
                    public byte BitsPerSample { get; }
                    public int PlanesNumber { get; }
                    public int CfaLayout { get; }
                    public int EncodingType { get; }
                    /// <remarks>waveletLevelsNumber</remarks>
                    public int ImageLevels { get; }
                    /// <remarks>tileHorizontallyCountFlag</remarks>
                    public int HasTileCols { get; }
                    /// <remarks>tileVerticallyCountFlag</remarks>
                    public int HasTileRows { get; }
                    public int MdatTrackHeaderSize { get; }
                    public bool HasExtendedHeaderSize { get; }
                }
                /// <remarks>CDI1</remarks>
                public class CanonDimensionsTag
                {
                    private const int _smallIad1Chunk = 0x28;
                    private const int _bigIad1Chunk = 0x38;

                    /// <remarks>IAD1 for size 0x28</remarks>
                    public class SmallImageAreaDimensions
                    {
                        public SmallImageAreaDimensions(ReadOnlyMemory<byte> memory)
                        {
                            var unknownZero1 = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                            var unknownZero2 = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);

                            var imageWidth = Int16Serializer.BigEndian.Deserialize(memory.Slice(4, 2).ToArray(), 0);
                            var imageHeight = Int16Serializer.BigEndian.Deserialize(memory.Slice(6, 2).ToArray(), 0);

                            var unknownOne1 = Int16Serializer.BigEndian.Deserialize(memory.Slice(8, 2).ToArray(), 0);

                            //0 (small), 2 (big) = flag for sliced
                            var flag = Int16Serializer.BigEndian.Deserialize(memory.Slice(10, 2).ToArray(), 0);

                            var unknownOne2 = Int16Serializer.BigEndian.Deserialize(memory.Slice(12, 2).ToArray(), 0);
                            var unknownZero3 = Int16Serializer.BigEndian.Deserialize(memory.Slice(14, 2).ToArray(), 0);

                            var unknownOne3 = Int16Serializer.BigEndian.Deserialize(memory.Slice(16, 2).ToArray(), 0);
                            var unknownZero4 = Int16Serializer.BigEndian.Deserialize(memory.Slice(18, 2).ToArray(), 0);

                            var width = Int16Serializer.BigEndian.Deserialize(memory.Slice(20, 2).ToArray(), 0);
                            var height = Int16Serializer.BigEndian.Deserialize(memory.Slice(22, 2).ToArray(), 0);

                            var unknownZero5 = Int16Serializer.BigEndian.Deserialize(memory.Slice(24, 2).ToArray(), 0);
                            var unknownZero6 = Int16Serializer.BigEndian.Deserialize(memory.Slice(26, 2).ToArray(), 0);


                            var width1 = Int16Serializer.BigEndian.Deserialize(memory.Slice(28, 2).ToArray(), 0);
                            var heigh2 = Int16Serializer.BigEndian.Deserialize(memory.Slice(30, 2).ToArray(), 0);

                            byte[] nameMemory = memory[4..].ToArray();
                            var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
                        }
                    }
                    /// <remarks>IAD1 for size 0x38</remarks>
                    public class BigImageAreaDimensions
                    {
                        public class BoxOffset
                        {
                            public short Left { get; init; }
                            public short Top { get; init; }
                            public short Right { get; init; }
                            public short Bottom { get; init; }
                        }

                        public BigImageAreaDimensions(ReadOnlyMemory<byte> memory)
                        {
                            var unknownZero1 = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                            var unknownZero2 = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);

                            ImageWidth = Int16Serializer.BigEndian.Deserialize(memory.Slice(4, 2).ToArray(), 0);
                            ImageHeight = Int16Serializer.BigEndian.Deserialize(memory.Slice(6, 2).ToArray(), 0);

                            var unknownOne1 = Int16Serializer.BigEndian.Deserialize(memory.Slice(8, 2).ToArray(), 0);

                            //0 (small), 2 (big) = flag for sliced
                            var flag = Int16Serializer.BigEndian.Deserialize(memory.Slice(10, 2).ToArray(), 0);

                            var unknownOne2 = Int16Serializer.BigEndian.Deserialize(memory.Slice(12, 2).ToArray(), 0);
                            var unknownZero3 = Int16Serializer.BigEndian.Deserialize(memory.Slice(14, 2).ToArray(), 0);

                            CropOffset = new BoxOffset
                            {
                                Left = Int16Serializer.BigEndian.Deserialize(memory.Slice(16, 2).ToArray(), 0), //sensorInfo[5]
                                Top = Int16Serializer.BigEndian.Deserialize(memory.Slice(18, 2).ToArray(), 0), //sensorInfo[6]
                                Right = Int16Serializer.BigEndian.Deserialize(memory.Slice(20, 2).ToArray(), 0), //sensorInfo[7]
                                Bottom = Int16Serializer.BigEndian.Deserialize(memory.Slice(22, 2).ToArray(), 0), //sensorInfo[8]
                            };

                            LeftOpticalBlackOffset = new BoxOffset
                            {
                                Left = Int16Serializer.BigEndian.Deserialize(memory.Slice(24, 2).ToArray(), 0),
                                Top = Int16Serializer.BigEndian.Deserialize(memory.Slice(26, 2).ToArray(), 0),
                                Right = Int16Serializer.BigEndian.Deserialize(memory.Slice(28, 2).ToArray(), 0),
                                Bottom = Int16Serializer.BigEndian.Deserialize(memory.Slice(30, 2).ToArray(), 0),
                            };

                            TopOpticalBlackOffset = new BoxOffset
                            {
                                Left = Int16Serializer.BigEndian.Deserialize(memory.Slice(32, 2).ToArray(), 0),
                                Top = Int16Serializer.BigEndian.Deserialize(memory.Slice(34, 2).ToArray(), 0),
                                Right = Int16Serializer.BigEndian.Deserialize(memory.Slice(36, 2).ToArray(), 0),
                                Bottom = Int16Serializer.BigEndian.Deserialize(memory.Slice(38, 2).ToArray(), 0),
                            };

                            ActiveAreaOffset = new BoxOffset
                            {
                                Left = Int16Serializer.BigEndian.Deserialize(memory.Slice(40, 2).ToArray(), 0),
                                Top = Int16Serializer.BigEndian.Deserialize(memory.Slice(42, 2).ToArray(), 0),
                                Right = Int16Serializer.BigEndian.Deserialize(memory.Slice(44, 2).ToArray(), 0),
                                Bottom = Int16Serializer.BigEndian.Deserialize(memory.Slice(46, 2).ToArray(), 0),
                            };
                        }

                        public short ImageWidth { get; }
                        public short ImageHeight { get; }

                        public BoxOffset CropOffset { get; }
                        public BoxOffset LeftOpticalBlackOffset { get; }
                        public BoxOffset TopOpticalBlackOffset { get; }
                        /// <remarks>
                        /// Active area is the rectangle containing valid pixel data. It has same meaning as ActiveArea DNG tag.
                        /// But for final crop, the crop rectangle must be used. This crops a little bit more pixels as active area.
                        /// Values are given as offsets (from zero), so you must + 1 if you want the amount of pixels.​
                        /// </remarks>
                        public BoxOffset ActiveAreaOffset { get; }
                    }

                    public CanonDimensionsTag(ReadOnlyMemory<byte> memory)
                    {
                        var fullBoxVersion = memory.Span[0];
                        var fullBoxFlags = memory.Slice(1, 3);

                        //Image Area Dimensions
                        var iad1Chunk = ChunkExtensions.ReadChunk(memory[4..]);

                        switch (iad1Chunk.Length)
                        {
                            case _smallIad1Chunk:
                                {
                                    SmallImage = new SmallImageAreaDimensions(iad1Chunk.Memory);
                                    break;
                                }
                            case _bigIad1Chunk:
                                {
                                    BigImage = new BigImageAreaDimensions(iad1Chunk.Memory);
                                    break;
                                }
                            default:
                                throw new InvalidOperationException($"IAD1 chunk length {iad1Chunk.Length} out of range.");
                        }
                    }

                    public SmallImageAreaDimensions SmallImage { get; }
                    public BigImageAreaDimensions BigImage { get; }
                }

                public CrawChunk(ReadOnlyMemory<byte> memory)
                {
                    var reserved = memory.Slice(0, 6); // 0
                    var dataReferenceIndex = UInt16Serializer.BigEndian.Deserialize(memory.Slice(6, 2).ToArray(), 0); // 1
                    var unknownZero = memory.Slice(8, 16); // 0

                    Width = UInt16Serializer.BigEndian.Deserialize(memory.Slice(24, 2).ToArray(), 0);
                    Height = UInt16Serializer.BigEndian.Deserialize(memory.Slice(26, 2).ToArray(), 0);

                    XResolution = UInt16Serializer.BigEndian.Deserialize(memory.Slice(28, 2).ToArray(), 0);
                    var xResolutionFraction = UInt16Serializer.BigEndian.Deserialize(memory.Slice(30, 2).ToArray(), 0);
                    Debug.Assert(xResolutionFraction == 0, $"{nameof(xResolutionFraction)} not null");

                    YResolution = UInt16Serializer.BigEndian.Deserialize(memory.Slice(32, 2).ToArray(), 0);
                    var yResolutionFraction = UInt16Serializer.BigEndian.Deserialize(memory.Slice(34, 2).ToArray(), 0);
                    Debug.Assert(yResolutionFraction == 0, $"{nameof(yResolutionFraction)} not null");

                    var unknown2 = Int32Serializer.BigEndian.Deserialize(memory.Slice(36, 4).ToArray(), 0); // 0
                    var unknown3 = UInt16Serializer.BigEndian.Deserialize(memory.Slice(40, 2).ToArray(), 0); // 1

                    byte[] compressorNameMemory = memory.Slice(42, 32).ToArray();
                    var compressorName = StringSerializer.Default.Deserialize(compressorNameMemory, 0, compressorNameMemory.Length);

                    BitsDepth = Int16Serializer.BigEndian.Deserialize(memory.Slice(74, 2).ToArray(), 0); // 24

                    var unknown4 = Int16Serializer.BigEndian.Deserialize(memory.Slice(76, 2).ToArray(), 0); // -1

                    var flags = Int16Serializer.BigEndian.Deserialize(memory.Slice(78, 2).ToArray(), 0); // 3 for Jpeg, 1 for craw/raw
                    var unknown5 = Int16Serializer.BigEndian.Deserialize(memory.Slice(80, 2).ToArray(), 0); // 0 for jpeg. 1 for craw/raw
                    Type = (ContainerType)unknown5;

                    var chunks = ChunkExtensions.ReadChunks(memory[82..].ToArray());
                    if (unknown5 == 0)
                    {
                        var jpegChunk = chunks["JPEG"];
                        var a = Int32Serializer.BigEndian.Deserialize(jpegChunk.Memory.ToArray(), 0);
                    }

                    if (Type == ContainerType.CrawOrRaw)
                    {
                        Compression = new CompressionTag(chunks["CMP1"].Memory);
                        CanonDimensions = new CanonDimensionsTag(chunks["CDI1"].Memory);
                    }

                    byte[] nameMemory = memory[82..].ToArray();
                    var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
                }

                public ushort Width { get; }
                public ushort Height { get; }
                public double XResolution { get; }
                public double YResolution { get; }
                public short BitsDepth { get; }
                public ContainerType Type { get; }
                public CompressionTag Compression { get; }
                public CanonDimensionsTag CanonDimensions { get; }
            }
            public class CtmdChunk
            {
                public struct Record
                {
                    public Record(ReadOnlyMemory<byte> memory)
                    {
                        Type = Int32Serializer.BigEndian.Deserialize(memory.Slice(0, 4).ToArray(), 0);
                        Size = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                    }

                    public int Type { get; }
                    public int Size { get; }

                    public override string ToString() => $"Type = {Type}, Size = {Size}";
                }

                public CtmdChunk(ReadOnlyMemory<byte> memory)
                {
                    var zero = Int32Serializer.BigEndian.Deserialize(memory.Slice(0, 4).ToArray(), 0); // 0
                    var one = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0); // 1
                    var recordsCount = Int32Serializer.BigEndian.Deserialize(memory.Slice(8, 4).ToArray(), 0);

                    Records = new Record[recordsCount];
                    for (int i = 0; i != Records.Length; i++)
                        Records[i] = new Record(memory.Slice(12 + i * 8, 8));

                    byte[] nameMemory = memory[12..].ToArray();
                    var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
                }

                public Record[] Records { get; }
            }
            /// <remarks>co64</remarks>
            public class ChunkOffset64Box
            {
                public ChunkOffset64Box(ReadOnlyMemory<byte> memory)
                {
                    Version = memory.Span[0];
                    Flags = memory.Slice(1, 3).ToArray();
                    Count = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                    Offset = Int64Serializer.BigEndian.Deserialize(memory.Slice(8, 8).ToArray(), 0);

                    if (Count != 1)
                        throw new NotImplementedException($"Not implemented for {nameof(Count)} != 1");
                }

                /// <summary>
                /// Specifies the version of this box.
                /// </summary>
                private byte Version { get; }
                /// <summary>
                /// Map of flags.
                /// </summary>
                private byte[] Flags { get; } = new byte[3];

                /// <summary>
                /// Gives the number of entries in the following table.
                /// </summary>
                public int Count { get; }
                /// <summary>
                /// Gives the offset of the start of a chunk into its containing media file.
                /// </summary>
                public long Offset { get; }
            }
            /// <remarks>stsz</remarks>
            public class SampleSizeBox
            {
                public SampleSizeBox(ReadOnlyMemory<byte> memory)
                {
                    Version = memory.Span[0];
                    Flags = memory.Slice(1, 3).ToArray();
                    SampleSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                    SampleCount = Int32Serializer.BigEndian.Deserialize(memory.Slice(8, 4).ToArray(), 0);

                    if (SampleSize == 0)
                    {
                        EntrySizes = new int[SampleCount];
                        var entrySizesOffset = 12;
                        for (int i = 0; i != SampleCount; i++)
                            EntrySizes[i] = Int32Serializer.BigEndian.Deserialize(memory.Slice(entrySizesOffset + i * 4, 4).ToArray(), 0);

                        Debug.Assert(SampleCount == 1, $"Undefined state {nameof(SampleCount)} != 1.");
                    }
                }

                /// <summary>
                /// Specifies version of this box.
                /// </summary>
                private byte Version { get; }
                /// <summary>
                /// Map of flags.
                /// </summary>
                private byte[] Flags { get; } = new byte[3];

                /// <summary>
                /// Specifies default sample size. 
                /// </summary>
                /// <remarks>
                /// Specifying the default sample size. If all the samples are the same size, 
                /// this field contains that size value.If this field is set to 0, then the samples have different sizes,
                /// and those sizes are stored in the sample size table. If this field is not 0, it specifies the constant
                /// sample size, and no array follows. 
                /// </remarks>
                public int SampleSize { get; }
                /// <summary>
                /// Number of samples in the track.
                /// </summary>
                /// <remarks>If sample‐size is 0, then it is also the number of entries in the following table.</remarks>
                public int SampleCount { get; }
                /// <summary>
                /// Specifying the size of a sample, indexed by its number.
                /// </summary>
                public int[] EntrySizes { get; }
            }

            public const string CrawStsd = "CRAW";
            public const string CtmdStsd = "CTMD";

            public SampleTableBox(ReadOnlyMemory<byte> memory)
            {
                var chunks = ChunkExtensions.ReadChunks(memory);

                var stsd = chunks["stsd"]; // Sample descriptions, codec types, init...
                var stsdChunk = ChunkExtensions.ReadChunk(stsd.Memory[8..]); // craw or ctmd
                if (stsdChunk.Name == CrawStsd)
                    Craw = new CrawChunk(stsdChunk.Memory);
                else
                    Ctmd = new CtmdChunk(stsdChunk.Memory);

                Size = new SampleSizeBox(chunks["stsz"].Memory); // Sample sizes, framing
                Offset = new ChunkOffset64Box(chunks["co64"].Memory); // pointer to picture
            }

            public CrawChunk Craw { get; }
            public CtmdChunk Ctmd { get; }
            public SampleSizeBox Size { get; }
            public ChunkOffset64Box Offset { get; }
        }

        public TrackBox(Chunk trakChunk)
        {
            var chunks = ChunkExtensions.ReadChunks(trakChunk.Memory);

            var mdiaChunks = ChunkExtensions.ReadChunks(chunks["mdia"].Memory);

            var minfChunks = ChunkExtensions.ReadChunks(mdiaChunks["minf"].Memory);

            SampleTable = new SampleTableBox(minfChunks["stbl"].Memory);
        }

        public SampleTableBox SampleTable { get; }
    }

    public class HandlerBox
    {
        public const string Vide = "vide";
        public const string Meta = "meta";

        static private readonly ArraySerializer<uint> _arraySerializer;

        static HandlerBox()
        {
            _arraySerializer = new(UInt32Serializer.BigEndian);
        }

        public HandlerBox(ReadOnlyMemory<byte> memory)
        {
            //PreDefined = UInt32Serializer.BigEndian.Deserialize(memory.Slice(0, 4).ToArray(), 0);
            //HandlerType = UInt32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
            //Reserved = _arraySerializer.Deserialize(memory.Slice(8, 12).ToArray(), 0, 3);

            byte[] nameMemory = memory.Slice(8, 4).ToArray();
            Name = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
        }

        //public uint PreDefined { get; }
        //public uint HandlerType { get; }
        //public uint[] Reserved { get; } = new uint[3];
        public string Name { get; }
    }

    public class CtboRecord
    {
        public CtboRecord(ReadOnlyMemory<byte> memory)
        {
            Index = UInt32Serializer.BigEndian.Deserialize(memory.Slice(0, 4).ToArray(), 0);
            Offset = Int64Serializer.BigEndian.Deserialize(memory.Slice(4, 8).ToArray(), 0);
            Size = Int64Serializer.BigEndian.Deserialize(memory.Slice(12, 8).ToArray(), 0);
        }

        public uint Index { get; }
        public long Offset { get; }
        public long Size { get; }
    }
}
