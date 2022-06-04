using Devdeb.Images.CanonRaw.Tests.Chunks;
using Devdeb.Serialization.Serializers.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Devdeb.Images.CanonRaw.Tests.Program;

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

        public class TileHeader
        {
            public static short[] Signatures { get; } = new short[] { unchecked((short)0xFF01), unchecked((short)0xFF11) };
            public TileHeader(ref ReadOnlyMemory<byte> memory)
            {
                var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                if (!Signatures.Contains(signature))
                    throw new InvalidOperationException($"Invalid tile header signature {signature}.");

                Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
                FF01DataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                Counter = memory.Span[8] >> 4;

                memory = memory[12..];
                PlaneHeaders = new List<PlaneHeader>();
                for (; PlaneHeader.TryParse(ref memory, out PlaneHeader planeHeader);)
                    PlaneHeaders.Add(planeHeader);
            }

            public short Size { get; }
            public int FF01DataSize { get; }
            public int Counter { get; }

            public List<PlaneHeader> PlaneHeaders { get; }
        }
        public class PlaneHeader
        {
            static public short[] Signatures { get; } = new short[] { unchecked((short)0xFF02), unchecked((short)0xFF12) };

            public PlaneHeader(ref ReadOnlyMemory<byte> memory)
            {
                Size = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
                PlaneDataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                Counter = memory.Span[8] >> 4;
                DoesSupportsPartialFlag = (memory.Span[8] & 8) != 0;
                RoundedBits = (memory.Span[8] >> 1) & 3; //0

                var compHdrRoundedBits = (memory.Span[8] >> 1) & 3;

                memory = memory[12..];
                SubbandHeader = new SubbandHeader(memory);
                memory = memory[12..];
            }

            public short Size { get; }
            /// <remarks>Sum of plane data equals size of parent tile.</remarks>
            public int PlaneDataSize { get; }
            public int Counter { get; }
            public bool DoesSupportsPartialFlag { get; }
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
        public class SubbandHeader
        {
            public static short[] Signatures { get; } = new short[] { unchecked((short)0xFF03), unchecked((short)0xFF13) };
            public SubbandHeader(ReadOnlyMemory<byte> memory)
            {
                var signature = Int16Serializer.BigEndian.Deserialize(memory.Slice(0, 2).ToArray(), 0);
                if (!Signatures.Contains(signature))
                    throw new InvalidOperationException($"Invalid subband header signature {signature}.");

                HdrSize = Int16Serializer.BigEndian.Deserialize(memory.Slice(2, 2).ToArray(), 0);
                SubbandDataSize = Int32Serializer.BigEndian.Deserialize(memory.Slice(4, 4).ToArray(), 0);
                Counter = memory.Span[8] >> 4; //0
                DoesSupportsPartialFlag = (memory.Span[8] >> 3) & 1; //0
                QParam = (byte)((memory.Span[8] << 5) | (memory.Span[9] >> 3)); //4
                Unknown = (memory.Span[9] & 7 << 16) | (memory.Span[10] << 8) | (memory.Span[11]); //2  

                var bitData = Int32Serializer.BigEndian.Deserialize(memory.Slice(8, 4).ToArray(), 0);
                DataSize = SubbandDataSize - (bitData & 0x7FFFF);
            }

            public short HdrSize { get; }
            /// <remarks>Sum of plane data equals size of parent tile.</remarks>
            public int SubbandDataSize { get; }
            public int Counter { get; }
            public int DoesSupportsPartialFlag { get; }
            /// <remarks>QuantValue</remarks>
            public byte QParam { get; }
            public int Unknown { get; }

            public CrxBandParam BandParam { get; set; } = null;
            public long DataSize { get; set; }
            public long DataOffset { get; set; } = 0;
            public int KParam { get; set; } = 0;
            public byte[] BandBuf { get; set; } = null;
            public int BandSize { get; set; } = 0;
            public int QStepBase { get; set; } = 0;
            public int QStepMult { get; set; } = 0;

            public long mdatOffset;
            public short Width;
            public short Height;
            public short rowStartAddOn;
            public short rowEndAddOn;
            public short colStartAddOn;
            public short colEndAddOn;
            public short levelShift;
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

            foreach (var plane in tileHeader.PlaneHeaders)
            {
                plane.SubbandHeader.Width = checked((short)hdr.TileWidth);
                plane.SubbandHeader.Height = checked((short)hdr.TileHeight);
            }

            var tileMemory = memory[ctmd.Craw.Compression.MdatTrackHeaderSize..];

            var planeOffset = 0;
            var redMemory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[0].PlaneDataSize);
            planeOffset += tileHeader.PlaneHeaders[0].PlaneDataSize;
            var green1Memory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[1].PlaneDataSize);
            planeOffset += tileHeader.PlaneHeaders[1].PlaneDataSize;
            var green2Memory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[2].PlaneDataSize);
            planeOffset += tileHeader.PlaneHeaders[2].PlaneDataSize;
            var blueMemory = tileMemory.Slice(planeOffset, tileHeader.PlaneHeaders[3].PlaneDataSize);

            var totalLEngth = redMemory.Length + green1Memory.Length + green2Memory.Length + blueMemory.Length;

            //Create(plane1Memory, plane2Memory, plane3Memory, plane4Memory)
            //    .Save(@"C:\Users\lehac\Desktop\test_raw_picture_red.jpg", ImageFormat.Jpeg);

            int[] incrBitTable = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0 };

            //img->planeWidth


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


            //img->planeBuf = 0;
            //img->outBufs[0] = img->outBufs[1] = img->outBufs[2] = img->outBufs[3] = 0;
            // cfa layout = 0 and planes = 4 
            // R G
            // G B
            //img->outBufs[0] = outBuf; //0 - R
            //img->outBufs[1] = outBuf + 1; //1 - G
            //img->outBufs[2] = outBuf + rowSize;  // 11136 - G
            //img->outBufs[3] = img->outBufs[2] + 1;  // 11137 - B
            var width = planeWidth - hdr.TileWidth * (tileCols - 1); // 5568
            var progrDataSize = sizeof(int) * planeWidth; // 22272
            var paramLength = 2 * planeWidth + 4; // 11140
            //var nonProgrData = progrDataSize ? paramData + paramLength : 0;
            //tile->hasQPData = false;
            //tile->mdatQPDataSize = 0;
            //tile->mdatExtraSize = 0;

            //band->kParam = 0;
            //band->bandParam = 0;
            //band->bandBuf = 0;
            //band->bandSize = 0;
            //band->dataSize = subbandSize - (bitData & 0x7FFFF);
            //band->supportsPartial = bitData & 0x8000000; //0
            //band->qParam = (bitData >> 19) & 0xFF; // 4
            //band->qStepBase = 0;
            //band->qStepMult = 0;

            //uint32_t bandHeight = tile->height;
            //uint32_t bandWidth = tile->width;
            //int32_t bandWidthExCoef = 0;
            //int32_t bandHeightExCoef = 0;

            //band->width = bandWidthExCoef + bandWidth; //tile->width
            //band->height = bandHeightExCoef + bandHeight; //tile->height
            DecodePlane(redMemory, hdr, tileHeader, 0);

            CreateBitmap(redMemory).Save(@"C:\Users\lehac\Desktop\test_raw_picture.jpg", ImageFormat.Jpeg);
            CreateBitmap(green1Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture2.jpg", ImageFormat.Jpeg);
            CreateBitmap(green2Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture3.jpg", ImageFormat.Jpeg);
            CreateBitmap(blueMemory).Save(@"C:\Users\lehac\Desktop\test_raw_picture4.jpg", ImageFormat.Jpeg);

            byte[] nameMemory = tileMemory.ToArray();
            var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
        }

        private static void DecodePlane(
            Memory<byte> plane,
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            TileHeader tileHeader,
            int planeNumber
        )
        {
            var bitStream = new ReadOnlyBitStream(plane);
            CrxBandParam param = crxSetupSubbandData(plane, hdr, tileHeader, planeNumber);

            var subbandHeader = tileHeader.PlaneHeaders[planeNumber].SubbandHeader;

            for (int i = 0; i < hdr.TileHeight; ++i)
            {
                crxDecodeLine(param, subbandHeader.BandBuf);
                //if (crxDecodeLine(planeComp->subBands->bandParam, planeComp->subBands->bandBuf))
                //    return -1;
                //int32_t* lineData = (int32_t*)planeComp->subBands->bandBuf;
                //crxConvertPlaneLine(img, imageRow + i, imageCol, planeNumber, lineData, tile->width);
            }
        }


        static void crxDecodeLine(CrxBandParam param, byte[] bandBuf)
        {
            if (param.CurLine == 0)
            {
                int lineLength = param.SubbandWidth + 2;

                param.SParam = 0;
                param.KParam = 0;
                if (param.SupportsPartial)
                {
                    if (param.RoundedBitsMask <= 0)
                    {
                        param.LineBuf0 = param.ParamData;
                        param.LineBuf1 = param.LineBuf0.Slice(0, lineLength);
                        int* lineBuf = param.LineBuf1 + 1;
                        if (crxDecodeTopLine(param))
                            return -1;
                        memcpy(bandBuf, lineBuf, param->subbandWidth * sizeof(int32_t));
                        ++param->curLine;
                    }
                    else
                    {
                        throw new NotImplementedException();
                        //param->roundedBits = 1;
                        //if (param->roundedBitsMask & ~1)
                        //{
                        //    while (param->roundedBitsMask >> param->roundedBits)
                        //        ++param->roundedBits;
                        //}
                        //param->lineBuf0 = (int32_t*)param->paramData;
                        //param->lineBuf1 = param->lineBuf0 + lineLength;
                        //int32_t* lineBuf = param->lineBuf1 + 1;
                        //if (crxDecodeTopLineRounded(param))
                        //    return -1;
                        //memcpy(bandBuf, lineBuf, param->subbandWidth * sizeof(int32_t));
                        //++param->curLine;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                    //param->lineBuf2 = (int32_t*)param->nonProgrData;
                    //param->lineBuf0 = (int32_t*)param->paramData;
                    //param->lineBuf1 = param->lineBuf0 + lineLength;
                    //int32_t* lineBuf = param->lineBuf1 + 1;
                    //if (crxDecodeTopLineNoRefPrevLine(param))
                    //    return -1;
                    //memcpy(bandBuf, lineBuf, param->subbandWidth * sizeof(int32_t));
                    //++param->curLine;
                }
            }
        }


        static CrxBandParam crxSetupSubbandData(
            Memory<byte> plane,
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            TileHeader tileHeader,
            int planeNumber
        )
        {
            long compDataSize = 0;
            long waveletDataOffset = 0;
            long compCoeffDataOffset = 0;
            var toSubbands = 3 * hdr.ImageLevels + 1; //1
            var transformWidth = 0;

            // calculate sizes
            //for (int32_t subbandNum = 0; subbandNum < toSubbands; subbandNum++)
            //{
            //    subbands[subbandNum].bandSize = subbands[subbandNum].width * sizeof(int32_t); // 4bytes
            //    compDataSize += subbands[subbandNum].bandSize;
            //}
            var bandSize = hdr.TileWidth * 4; // 11136
            tileHeader.PlaneHeaders[planeNumber].SubbandHeader.BandSize = bandSize;
            compDataSize += bandSize;// 11136

            //planeComp->compBuf = (uint8_t *)img->memmgr.malloc(compDataSize);
            byte[] compBuf = new byte[compDataSize];

            // subbands buffer and sizes initialisation
            //uint64_t subbandMdatOffset = img->mdatOffset + mdatOffset; // Поидеи начало subband
            //uint8_t* subbandBuf = planeComp->compBuf; // буфер, который инициализировали выше на 1 строку * 4 байта.
            var subbandBuf = compBuf;

            //for (int32_t subbandNum = 0; subbandNum < toSubbands; subbandNum++)
            //{
            //    subbands[subbandNum].bandBuf = subbandBuf; //дублирование ссылки в subbands
            //    subbandBuf += subbands[subbandNum].bandSize;
            //    subbands[subbandNum].mdatOffset = subbandMdatOffset + subbands[subbandNum].dataOffset;
            //}
            //subbandBuf теперь указывает на конец буфера
            tileHeader.PlaneHeaders[planeNumber].SubbandHeader.BandBuf = subbandBuf;

            // decoding params and bitstream initialisation
            //for (int32_t subbandNum = 0; subbandNum < toSubbands; subbandNum++)
            //{ 
            //    if (subbands[subbandNum].dataSize) // вроде как должны быть данные
            //    {
            //        bool supportsPartial = false; // true
            //        uint32_t roundedBitsMask = 0;

            //        if (planeComp->supportsPartial && subbandNum == 0) // тут будет true
            //        {
            //            roundedBitsMask = planeComp->roundedBitsMask; // 0 будет
            //            supportsPartial = true;
            //        }
            //        if (crxParamInit(img, &subbands[subbandNum].bandParam, subbands[subbandNum].mdatOffset,
            //                         subbands[subbandNum].dataSize, subbands[subbandNum].width, subbands[subbandNum].height,
            //                         supportsPartial, roundedBitsMask))
            //            return -1;
            //    }
            //}
            return crxParamInit(plane, hdr, tileHeader, planeNumber);
        }

        static CrxBandParam crxParamInit(
            Memory<byte> plane,
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            TileHeader tileHeader,
            int planeNumber,
            bool supportsPartial = true,
            int roundedBitsMask = 0
        )
        {
            //int32_t progrDataSize = supportsPartial ? 0 : sizeof(int32_t) * subbandWidth;
            //int32_t paramLength = 2 * subbandWidth + 4;
            //uint8_t* paramBuf = 0;
            var progrDataSize = supportsPartial ? 0 : sizeof(int) * hdr.TileWidth; // 0
            var paramLength = 2 * hdr.TileWidth + 4; //11140
            Debug.Assert(progrDataSize == 0);


            // paramBuf = (uint8_t *)img->memmgr.calloc(1, sizeof(CrxBandParam) + sizeof(int32_t) * paramLength + progrDataSize);
            //*param = (CrxBandParam*)paramBuf;
            int[] paramBuf = new int[paramLength + progrDataSize];

            //paramBuf += sizeof(CrxBandParam);

            //(*param)->paramData = (int32_t*)paramBuf; 
            //(*param)->nonProgrData = progrDataSize ? (*param)->paramData + paramLength : 0;
            //(*param)->subbandWidth = subbandWidth;
            //(*param)->subbandHeight = subbandHeight;
            //(*param)->roundedBits = 0;
            //(*param)->curLine = 0;
            //(*param)->roundedBitsMask = roundedBitsMask;
            //(*param)->supportsPartial = supportsPartial;
            //(*param)->bitStream.bitData = 0;
            //(*param)->bitStream.bitsLeft = 0;
            //(*param)->bitStream.mdatSize = subbandDataSize; // видимо размер subband
            //(*param)->bitStream.curPos = 0;
            //(*param)->bitStream.curBufSize = 0;
            //(*param)->bitStream.curBufOffset = subbandMdatOffset; // видимо текущий адрес начала subband
            //(*param)->bitStream.input = img->input;

            //crxFillBuffer(&(*param)->bitStream);

            CrxBandParam param = new();
            param.ParamData = paramBuf;
            param.NonProgrData = null;
            param.SubbandWidth = checked((short)hdr.TileWidth);
            param.SubbandHeight = checked((short)hdr.TileHeight);
            param.RoundedBits = 0;
            param.CurLine = 0;
            param.RoundedBitsMask = roundedBitsMask;
            param.SupportsPartial = supportsPartial;
            param.BitStream.BitData = 0;
            param.BitStream.BitsLeft = 0;
            param.BitStream.MdatSize = tileHeader.PlaneHeaders[planeNumber].SubbandHeader.SubbandDataSize; // видимо размер subband
            param.BitStream.CurPos = 0;
            param.BitStream.CurBufSize = 0;
            param.BitStream.CurBufOffset = 0; // видимо текущий адрес начала subband
            //param.BitStream.Input = img->input;
            param.BitStream.Input = plane;

            param.BitStream.FillBuffer();

            return param;
        }


        public class CrxBitstream
        {
            public const int CRX_BUF_SIZE = 0x10000;

            /// <summary>
            /// Temporary buffer for reading subband data.
            /// </summary>
            public byte[] MdatBuf = new byte[CRX_BUF_SIZE];
            /// <summary>
            /// Subband size.
            /// </summary>
            public int MdatSize;
            /// <summary>
            /// Current subband offset.
            /// </summary>
            public int CurBufOffset;
            public uint CurPos;
            /// <summary>
            /// Count available bytes in <see cref="MdatBuf"/>.
            /// </summary>
            public uint CurBufSize;
            public uint BitData;
            public int BitsLeft;
            //LibRaw_abstract_datastream* input;
            public Memory<byte> Input;

            public void FillBuffer()
            {
                if (CurPos >= CurBufSize && MdatSize != 0)
                {
                    var size = Math.Min(CRX_BUF_SIZE, MdatSize);
                    MdatBuf = Input.Slice(CurBufOffset, size).ToArray();
                    MdatSize -= size;
                }
            }
        };

        public unsafe class CrxBandParam
        {
            public CrxBitstream BitStream { get; set; } = new();
            public short SubbandWidth { get; set; }
            public short SubbandHeight { get; set; }
            public int RoundedBitsMask { get; set; }
            public int RoundedBits { get; set; }
            public short CurLine { get; set; }
            public Memory<int> LineBuf0 { get; set; }
            public Memory<int> LineBuf1 { get; set; }
            public Memory<int> LineBuf2 { get; set; }
            public int SParam { get; set; }
            public int KParam { get; set; }
            public int[] ParamData { get; set; }
            public int[] NonProgrData { get; set; } = null;
            public bool SupportsPartial { get; set; }
        };




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
        private int _bitOffset;

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
