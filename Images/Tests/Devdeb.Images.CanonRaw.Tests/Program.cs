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
            var redMemory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[0].SubbandHeader.DataSize);
            planeOffset += tileHeader.PlaneHeaders[0].PlaneDataSize;
            var green1Memory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[1].SubbandHeader.DataSize);
            planeOffset += tileHeader.PlaneHeaders[1].PlaneDataSize;
            var green2Memory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[2].SubbandHeader.DataSize);
            planeOffset += tileHeader.PlaneHeaders[2].PlaneDataSize;
            var blueMemory = tileMemory.Slice(planeOffset, (int)tileHeader.PlaneHeaders[3].SubbandHeader.DataSize);

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
            //var width = planeWidth - hdr.TileWidth * (tileCols - 1); // 5568
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
            //band->suppor
            //tsPartial = bitData & 0x8000000; //0
            //band->qParam = (bitData >> 19) & 0xFF; // 4
            //band->qStepBase = 0;
            //band->qStepMult = 0;

            //uint32_t bandHeight = tile->height;
            //uint32_t bandWidth = tile->width;
            //int32_t bandWidthExCoef = 0;
            //int32_t bandHeightExCoef = 0;

            //band->width = bandWidthExCoef + bandWidth; //tile->width
            //band->height = bandHeightExCoef + bandHeight; //tile->height
            var red = DecodePlane(redMemory, hdr, tileHeader, 0);
            var green1 = DecodePlane(green1Memory, hdr, tileHeader, 1);
            var green2 = DecodePlane(green2Memory, hdr, tileHeader, 2);
            var blue = DecodePlane(blueMemory, hdr, tileHeader, 3);


            var subbandWidth = red[0].Length;
            var subbandHeight = red.Count;


            Bitmap bitmap = new(subbandWidth, subbandHeight);

            var max_val = (1 << 14) - 1;

            for (int height = 0; height != subbandHeight; height++)
                for (int width = 0; width != subbandWidth; width++)
                {
                    var redValue = ((double)red[height][width] / max_val) * 255;
                    var green1Value = ((double)green1[height][width] / max_val) * 255 / 2;
                    var green2Value = ((double)green2[height][width] / max_val) * 255;
                    var blueValue = ((double)blue[height][width] / max_val) * 255;

                    bitmap.SetPixel(width, height, Color.FromArgb((int)redValue, (int)green1Value, (int)blueValue));
                }
            bitmap.Save($@"C:\Users\lehac\Desktop\bier.png", ImageFormat.Png);



            //Bitmap bitmap = new(subbandWidth * 2, subbandHeight * 2);

            //var max_val = (1 << 14) - 1;

            //for (int height = 0; height != subbandHeight - 1; height++)
            //    for (int width = 0; width != subbandWidth - 1; width++)
            //    {
            //        var redValue = ((double)red[height][width] / max_val) * 255;
            //        var green1Value = ((double)green1[height][width] / max_val) * 255;
            //        var green2Value = ((double)green2[height][width] / max_val) * 255;
            //        var blueValue = ((double)blue[height][width] / max_val) * 255;

            //        bitmap.SetPixel(width * 2, height * 2, Color.FromArgb((int)redValue, 0, 0));
            //        bitmap.SetPixel(width * 2 + 1, height * 2, Color.FromArgb(0, (int)green1Value, 0));
            //        bitmap.SetPixel(width * 2, height * 2 + 1, Color.FromArgb(0, (int)green2Value, 0));
            //        bitmap.SetPixel(width * 2 + 1, height * 2 + 1, Color.FromArgb(0, 0, (int)blueValue));
            //    }
            //bitmap.Save($@"C:\Users\lehac\Desktop\bier.png", ImageFormat.Png);

            CreateBitmap(redMemory).Save(@"C:\Users\lehac\Desktop\test_raw_picture.jpg", ImageFormat.Jpeg);
            CreateBitmap(green1Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture2.jpg", ImageFormat.Jpeg);
            CreateBitmap(green2Memory).Save(@"C:\Users\lehac\Desktop\test_raw_picture3.jpg", ImageFormat.Jpeg);
            CreateBitmap(blueMemory).Save(@"C:\Users\lehac\Desktop\test_raw_picture4.jpg", ImageFormat.Jpeg);

            byte[] nameMemory = tileMemory.ToArray();
            var str = StringSerializer.Default.Deserialize(nameMemory, 0, nameMemory.Length);
        }

        /// Decode top line without a previous K buffer
        private static void decode_top_line_no_ref_prev_line(
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            DecoderCoefficients coefficients,
            RiceDecoder riceDecoder,
            ref uint sParam
        )
        {
            Debug.Assert(coefficients.LinePosition == 1);
            var remaining = (uint)hdr.TileWidth;
            for (; remaining > 1;)
            {
                if (coefficients.A != 0)
                {
                    var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                    coefficients.X = ErrorCodeSigned(bitCode);
                }
                else
                {
                    if (riceDecoder.BitStream.Read(1, out uint value) && value == 1)
                    {
                        var nSyms = RunLengthDecoder.SymbolRunCount(ref sParam, riceDecoder, remaining);
                        remaining = Extensions.SaturatingSub(remaining, nSyms);
                        // copy symbol n_syms times
                        for (uint counter = 0; counter != nSyms; counter++)
                        {
                            // For the first line, run-length coding uses only the symbol
                            // value 0, so we can fill the line buffer and K buffer with 0.
                            coefficients.X = 0;
                            coefficients.KBuffer[coefficients.LinePosition - 1] = 0;
                            coefficients.MoveLinePosition();
                        }

                        if (remaining == 0)
                            break;
                    }
                    var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                    coefficients.X = ErrorCodeSigned(bitCode + 1);
                }
                coefficients.KBuffer[coefficients.LinePosition - 1] = riceDecoder.K;
                coefficients.MoveLinePosition();
                remaining = Extensions.SaturatingSub(remaining, 1);
            }
            if (remaining == 1)
            {
                var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                coefficients.X = ErrorCodeSigned(bitCode);
                coefficients.KBuffer[coefficients.LinePosition - 1] = riceDecoder.K;
                coefficients.MoveLinePosition();
            }
            Debug.Assert(coefficients.RightEndPosition);
            coefficients.X = 0;
        }

        /// Decode nontop line with a previous K buffer
        private static void decode_nontop_line_no_ref_prev_line(
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            DecoderCoefficients coefficients,
            RiceDecoder riceDecoder,
            ref uint sParam
        )
        {
            Debug.Assert(coefficients.LinePosition == 1);
            var remaining = (uint)hdr.TileWidth;
            for (; remaining > 1;)
            {
                if ((coefficients.D | coefficients.B | coefficients.A) != 0)
                {
                    var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, 0);
                    coefficients.X = ErrorCodeSigned(bitCode);
                    if (Extensions.SaturatingSub((uint)coefficients.KBuffer[coefficients.LinePosition], (uint)riceDecoder.K) <= 1)
                    {
                        if (riceDecoder.K >= 15)
                            riceDecoder.K = 15;
                    }
                    else
                        riceDecoder.K++;
                }
                else
                {
                    if (riceDecoder.BitStream.Read(1, out uint value) && value == 1)
                    {
                        Debug.Assert(remaining != 1);
                        var nSyms = RunLengthDecoder.SymbolRunCount(ref sParam, riceDecoder, remaining);
                        remaining = Extensions.SaturatingSub(remaining, nSyms);
                        // copy symbol n_syms times
                        for (uint counter = 0; counter != nSyms; counter++)
                        {
                            // For the first line, run-length coding uses only the symbol
                            // value 0, so we can fill the line buffer and K buffer with 0.
                            coefficients.X = 0;
                            coefficients.KBuffer[coefficients.LinePosition - 1] = 0;
                            coefficients.MoveLinePosition();
                        }
                    }

                    if (remaining <= 1)
                    {
                        if (remaining == 1)
                        {
                            var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                            coefficients.X = ErrorCodeSigned(bitCode + 1);
                            coefficients.KBuffer[coefficients.LinePosition - 1] = riceDecoder.K;
                            coefficients.MoveLinePosition();
                            remaining = Extensions.SaturatingSub(remaining, 1);// skip remaining check at end of function
                        }
                        break;
                    }
                    else
                    {
                        var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, 0);
                        coefficients.X = ErrorCodeSigned(bitCode + 1);// Caution: + 1
                        if (Extensions.SaturatingSub((uint)coefficients.KBuffer[coefficients.LinePosition], (uint)riceDecoder.K) <= 1)
                        {
                            if (riceDecoder.K >= 15)
                                riceDecoder.K = 15;
                        }
                        else
                            riceDecoder.K++;
                    }
                }
                coefficients.KBuffer[coefficients.LinePosition - 1] = riceDecoder.K;
                coefficients.MoveLinePosition();
                remaining = Extensions.SaturatingSub(remaining, 1);
            }
            if (remaining == 1)
            {
                var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                coefficients.X = ErrorCodeSigned(bitCode);
                coefficients.KBuffer[coefficients.LinePosition - 1] = riceDecoder.K;
                coefficients.MoveLinePosition();
            }
            Debug.Assert(coefficients.RightEndPosition);
        }

        /// Decode top line
        /// For the first line (top) in a plane, no MED is used because
        /// there is no previous line for coeffs b, c and d.
        /// So this decoding is a simplified version from decode_nontop_line().
        private static void decode_top_line(
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            DecoderCoefficients coefficients,
            RiceDecoder riceDecoder,
            ref uint sParam
        )
        {
            Debug.Assert(coefficients.LinePosition == 1);
            var remaining = (uint)hdr.TileWidth;
            coefficients.A = 0;
            for (; remaining > 1;)
            {
                if (coefficients.A != 0)
                    coefficients.X = coefficients.A;
                else
                {
                    if (riceDecoder.BitStream.Read(1, out uint value) && value == 1)
                    {
                        var nSyms = RunLengthDecoder.SymbolRunCount(ref sParam, riceDecoder, remaining);
                        remaining = Extensions.SaturatingSub(remaining, nSyms);
                        // copy symbol n_syms times
                        for (uint counter = 0; counter != nSyms; counter++)
                        {
                            coefficients.X = coefficients.A;
                            coefficients.MoveLinePosition();
                        }
                        if (remaining == 0)
                            break;
                    }
                    coefficients.X = 0;
                }

                var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                coefficients.X += ErrorCodeSigned(bitCode);
                coefficients.MoveLinePosition();
                remaining = Extensions.SaturatingSub(remaining, 1);
            }

            if (remaining == 1)
            {
                var x = coefficients.A; // no MED, just use coeff a
                var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                coefficients.X = x + ErrorCodeSigned(bitCode);
                coefficients.MoveLinePosition();
            }
            Debug.Assert(coefficients.RightEndPosition);
            coefficients.X = coefficients.A + 1;
        }

        /// Decode a line which is not a top line
        /// This used run length coding, Median Edge Detection (MED) and
        /// adaptive Golomb-Rice entropy encoding.
        /// Golomb-Rice becomes more efficient when using an adaptive K value
        /// instead of a fixed one.
        /// The K parameter is used as q = n >> k where n is the sample to encode.
        private static void decode_nontop_line(
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            DecoderCoefficients coefficients,
            RiceDecoder riceDecoder,
            ref uint sParam
        )
        {
            Debug.Assert(coefficients.LinePosition == 1);
            var remaining = (uint)hdr.TileWidth;
            coefficients.A = coefficients.B;

            for (; remaining > 1;)
            {
                int x = 0;
                //  c b d
                //  a x n
                // Median Edge Detection to predict pixel x. Described in patent US2016/0323602 and T.87
                if (coefficients.A == coefficients.B && coefficients.A == coefficients.D)
                {
                    // different than step [0104], where Condition: "a=c and c=b and b=d", c not used
                    if (riceDecoder.BitStream.Read(1, out uint value) && value == 1)
                    {
                        var nSyms = RunLengthDecoder.SymbolRunCount(ref sParam, riceDecoder, remaining);
                        remaining = Extensions.SaturatingSub(remaining, nSyms);
                        // copy symbol n_syms times
                        for (uint counter = 0; counter != nSyms; counter++)
                        {
                            coefficients.X = coefficients.A;
                            coefficients.MoveLinePosition();
                        }
                    }
                    if (remaining > 0)
                    {
                        x = coefficients.B; // use new coeff b because we moved line_pos!
                    }
                }
                else
                {
                    // no run length coding, use MED instead
                    x = MedianEdgeDetection(coefficients.A, coefficients.B, coefficients.C);
                }

                if (remaining > 0)
                {
                    var bitCode = riceDecoder.AdaptiveRiceDecode(false, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                    // add converted (+/-) error code to predicted value
                    coefficients.X = x + ErrorCodeSigned(bitCode);
                    // for not end of the line - use one symbol ahead to estimate next K
                    if (remaining > 1)
                    {
                        var delta = (coefficients.D - coefficients.B) << 1;
                        bitCode = (bitCode + (uint)Math.Abs(delta)) >> 1;
                    }
                    riceDecoder.UpdateK(bitCode, PREDICT_K_MAX);
                    coefficients.MoveLinePosition();
                }
                remaining = Extensions.SaturatingSub(remaining, 1);
            }

            if (remaining == 1)
            {
                int x = MedianEdgeDetection(coefficients.A, coefficients.B, coefficients.C);
                var bitCode = riceDecoder.AdaptiveRiceDecode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX);
                // add converted (+/-) error code to predicted value
                coefficients.X = x + ErrorCodeSigned(bitCode);
                coefficients.MoveLinePosition();
            }
            Debug.Assert(coefficients.RightEndPosition);
            coefficients.X = coefficients.A + 1;
        }

        const int PREDICT_K_MAX = 15;
        const int PREDICT_K_ESCAPE = 41;
        const int PREDICT_K_ESCBITS = 21;

        private static List<ushort[]> DecodePlane(
            Memory<byte> planeMemory,
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            TileHeader tileHeader,
            int planeNumber
        )
        {
            //var bitStream = new ReadOnlyBitStream(plane);
            //CrxBandParam param = crxSetupSubbandData(plane, hdr, tileHeader, planeNumber);

            var plane = tileHeader.PlaneHeaders[planeNumber];

            /// |E|Samples........................|E|
            /// |c|bd                           cb|d|
            /// |a|xn                           ax|n|
            ///  ^ ^                               ^
            ///  | |                               |-- Extra sample to provide fake d coefficent
            ///  | |---- First sample value
            ///  |------ Extra sample to provide a fake a/c coefficent

            uint sParam = 0;
            var subbandWidth = tileHeader.PlaneHeaders[0].SubbandHeader.Width;
            var subbandHeight = tileHeader.PlaneHeaders[0].SubbandHeader.Height;
            var coefficients = new DecoderCoefficients(1 + hdr.TileWidth + 1);
            var riceDecoder = new RiceDecoder(new ReadOnlyBitStream(planeMemory));
            bool supportsPartial = true;
            int rounded_bits_mask = 0;
            int rounded_bits = 0;


            for (int currentLine = 0; currentLine < hdr.TileHeight; currentLine++)
            {
                try
                {
                    if (currentLine == 0)
                    {
                        sParam = 0;
                        riceDecoder.K = 0;

                        if (supportsPartial)
                        {
                            if (rounded_bits_mask <= 0)
                                decode_top_line(hdr, coefficients, riceDecoder, ref sParam);
                            else
                            {
                                rounded_bits = 1;
                                //if ((rounded_bits_mask & !1) != 0)  
                                //{
                                //    while (rounded_bits_mask >> rounded_bits != 0) 
                                //    {
                                //        rounded_bits += 1;
                                //    }
                                //}
                                //self.decode_top_line_rounded(param) ?;
                            }
                        }
                        else
                            decode_top_line_no_ref_prev_line(hdr, coefficients, riceDecoder, ref sParam);
                    }
                    else if (!supportsPartial)
                    {
                        coefficients.SwapBuffers();
                        decode_nontop_line_no_ref_prev_line(hdr, coefficients, riceDecoder, ref sParam);
                    }
                    else if (rounded_bits_mask <= 0)
                    {
                        coefficients.SwapBuffers();
                        decode_nontop_line(hdr, coefficients, riceDecoder, ref sParam);
                    }
                    else
                    {
                        coefficients.SwapBuffers();
                        //self.decode_nontop_line_rounded(param) ?;
                    }
                }
                catch (Exception e)
                {
                    coefficients.Print(hdr.TileWidth, hdr.TileHeight);
                }
            }


            var median = 1 << (hdr.BitsPerSample - 1);
            var max_val = (1 << hdr.BitsPerSample) - 1;

            var newCoefficients = new List<ushort[]>();
            foreach (int[] planeLine in coefficients.SwappedBuffers)
            {
                ushort[] convertedLine = new ushort[planeLine.Length];

                for (int i = 0; i < planeLine.Length; i++)
                {
                    convertedLine[i] = constrain(median + planeLine[i], 0, max_val);
                }
                newCoefficients.Add(convertedLine);
            }

            //Bitmap bitmap = new(plane.SubbandHeader.Width, plane.SubbandHeader.Height);
            //for (int height = 0; height != bitmap.Height - 1; height++)
            //    for (int width = 0; width != bitmap.Width - 1; width++)
            //    {
            //        var redValue = ((double)newCoefficients[height][width] / max_val) * 255;
            //        bitmap.SetPixel(width, height, Color.FromArgb((int)redValue, 0, 0));
            //    }
            //bitmap.Save($@"C:\Users\lehac\Desktop\channel{planeNumber}.png", ImageFormat.Png);
            return newCoefficients;
        }

        private static ushort constrain(int Value, int minValue, int maxValue)
        {
            return (ushort)Math.Min(Math.Max(Value, minValue), maxValue);
        }

        /// The error code contains a sign bit at bit 0.
        /// Example: 10010 1 -> negative value, 10010 0 -> positive value
        /// This routine converts an unsigned bit_code to the correct
        /// signed integer value.
        /// For this, the sign bit is inverted and XOR with
        /// the shifted integer value.
        /// 15361 = -7681
        /// 15360 = 7680
        private static int ErrorCodeSigned(uint bitCode) => (int)(0 - (bitCode & 1) ^ (bitCode >> 1));

        /// <summary>
        /// Median Edge Detection
        /// [0053] Obtains a predictive value p of the coefficient by using
        /// MED prediction, thereby performing predictive coding.
        /// </summary>
        /// <returns>P coefficient</returns>
        static int MedianEdgeDetection(int coefficientA, int coefficientB, int coefficientC)
        {
            var minAB = Math.Min(coefficientA, coefficientB);
            var maxAB = Math.Max(coefficientA, coefficientB);

            if (coefficientC <= minAB)
                return maxAB;
            else if (coefficientC >= maxAB)
                return minAB;
            else
                return coefficientA + coefficientB - coefficientC;
        }

        public static class RunLengthDecoder
        {
            /// See ITU T.78 Section A.2.1 Step 3
            /// Initialise the variables for the run mode: RUNindex=0 and J[0..31]
            public static readonly int[] J = new int[32]
            {
                0, 0,  0,  0,  1,  1,  1,  1,
                2, 2,  2,  2,  3,  3,  3,  3,
                4, 4,  5,  5,  6,  6,  7,  7,
                8, 9, 10, 11, 12, 13, 14, 15
            };

            /// Precalculated values for (1 << J[0..31])
            public static readonly uint[] JSHIFT = new uint[32]
            {
                1u << J[0],  1u << J[1],  1u << J[2],  1u << J[3],
                1u << J[4],  1u << J[5],  1u << J[6],  1u << J[7],
                1u << J[8],  1u << J[9],  1u << J[10], 1u << J[11],
                1u << J[12], 1u << J[13], 1u << J[14], 1u << J[15],
                1u << J[16], 1u << J[17], 1u << J[18], 1u << J[19],
                1u << J[20], 1u << J[21], 1u << J[22], 1u << J[23],
                1u << J[24], 1u << J[25], 1u << J[26], 1u << J[27],
                1u << J[28], 1u << J[29], 1u << J[30], 1u << J[31]
            };

            /// Get symbol run count for run-length decoding
            /// See T.87 Section A.7.1.2 Run-length coding
            public static uint SymbolRunCount(ref uint sParam, RiceDecoder decoder, uint remaining)
            {
                uint run_cnt = 1;
                // See T.87 A.7.1.2 Code segment A.15
                // Bitstream 111110... means 5 lookups into J to decode final RUNcnt
                while (run_cnt != remaining && decoder.BitStream.Read(1, out var value) && value == 1)
                {
                    // JS is precalculated (1 << J[RUNindex])
                    run_cnt += JSHIFT[sParam];
                    if (run_cnt > remaining)
                    {
                        run_cnt = remaining;
                        break;
                    }
                    sParam = Math.Min(sParam + 1, 31);
                }
                // See T.87 A.7.1.2 Code segment A.16
                if (run_cnt < remaining)
                {
                    if (J[sParam] > 0)
                    {
                        Debug.Assert(decoder.BitStream.Read(J[sParam], out uint value));
                        run_cnt += value;
                    }
                    sParam = Extensions.SaturatingSub(sParam, 1); // prevent underflow

                    if (run_cnt > remaining)
                        throw new Exception("Crx decoder error while decoding line");
                }
                return run_cnt;
            }
        }

        public static class Extensions
        {
            public static uint SaturatingSub(uint a, uint b) => b > a ? 0 : a - b;
        }

        public class DecoderCoefficients
        {
            private int[] _previousLine;
            private int[] _currentLine;
            private int[] _kBuffer;

            private int _linePosition;
            private readonly int _size;

            public DecoderCoefficients(int size)
            {
                _previousLine = new int[size];
                _currentLine = new int[size];
                _kBuffer = new int[size];
                _size = size;
                _linePosition = 1;
            }

            public void SwapBuffers()
            {
                SwappedBuffers.Add(_currentLine.Skip(1).SkipLast(1).ToArray());
                var memory = _previousLine;
                _previousLine = _currentLine;
                _currentLine = memory;
                Array.Clear(_currentLine, 0, _currentLine.Length);
                _linePosition = 1;
            }

            public void MoveLinePosition() => _linePosition++;

            public int LinePosition => _linePosition;
            public bool RightEndPosition => _linePosition == _size - 1;

            public int[] KBuffer => _kBuffer;
            public List<int[]> SwappedBuffers { get; } = new();

            public void Print(int pictureWidth, int pictureHeight)
            {
                Bitmap bitmap = new(pictureWidth, pictureHeight);
                for (int width = 0; width != bitmap.Width; width++)
                    for (int height = 0; height != bitmap.Height; height++)
                        bitmap.SetPixel(width, height, Color.White);

                var decodedPixelsCount = SwappedBuffers.Count * (_size - 2);

                for (int width = 0; width != bitmap.Width; width++)
                    for (int height = 0; height != bitmap.Height; height++)
                    {
                        var index = width * height;
                        if (index >= decodedPixelsCount)
                        {
                            bitmap.Save(@"C:\Users\lehac\Desktop\decoded_file.png", ImageFormat.Png);
                            return;
                        }
                        bitmap.SetPixel(width, height, Color.FromArgb(SwappedBuffers[index / (_size - 2)][index % (_size - 2)], 0, 0));
                    }

                throw new Exception();
            }

            public int C
            {
                get => _previousLine[_linePosition - 1];
                set => _previousLine[_linePosition - 1] = value;
            }
            public int B
            {
                get => _previousLine[_linePosition];
                set => _previousLine[_linePosition] = value;
            }
            public int D
            {
                get => _previousLine[_linePosition + 1];
                set => _previousLine[_linePosition + 1] = value;
            }
            public int A
            {
                get => _currentLine[_linePosition - 1];
                set => _currentLine[_linePosition - 1] = value;
            }
            public int X
            {
                get => _currentLine[_linePosition];
                set => _currentLine[_linePosition] = value;
            }
            public int N
            {
                get => _currentLine[_linePosition + 1];
                set => _currentLine[_linePosition + 1] = value;
            }
        }

        public class RiceDecoder
        {
            private readonly ReadOnlyBitStream _bitStream;

            public RiceDecoder(ReadOnlyBitStream readOnlyBitStream)
            {
                _bitStream = readOnlyBitStream ?? throw new ArgumentNullException(nameof(readOnlyBitStream));
            }

            public int K { get; set; }
            public ReadOnlyBitStream BitStream => _bitStream;

            /// Golomb-Rice decoding
            /// https://w3.ual.es/~vruiz/Docencia/Apuntes/Coding/Text/03-symbol_encoding/09-Golomb_coding/index.html
            /// escape and esc_bits are used to interrupt decoding when
            /// a value is not encoded using Golomb-Rice but directly encoded
            /// by esc_bits bits.
            public uint RiceDecode(int escape, int escapeBits)
            {
                //q, quotient = n//m, with m = 2^k (Rice coding)
                var prefix = BitstreamZeros();
                if (prefix >= escape)
                {
                    // n
                    Debug.Assert(_bitStream.Read(escapeBits, out var value));
                    return value;
                }
                else if (K > 0)
                {
                    // Golomb-Rice coding : n = q * 2^k + r, with r is next k bits. r is n - (q*2^k)
                    Debug.Assert(_bitStream.Read(K, out var value));
                    return (prefix << K) | value;
                }
                else
                {
                    // q
                    return prefix;
                }
            }

            //let bit_code = p.rice.adaptive_rice_decode(true, PREDICT_K_ESCAPE, PREDICT_K_ESCBITS, PREDICT_K_MAX)?;
            /// Adaptive Golomb-Rice decoding, by adapting k value
            /// Sometimes adapting is based on the next coefficent (n) instead
            /// of current (x) coefficent. So you can disable it with `adapt_k`
            /// and update k later.
            /// Returns bit code.
            public uint AdaptiveRiceDecode(
                bool adaptK,
                int escape,
                int escapeBits,
                int maxK
            )
            {
                var result = RiceDecode(escape, escapeBits); //returns bit code.
                if (adaptK)
                    K = PredictKParamMax(K, result, maxK);
                return result;
            }

            public void UpdateK(uint bitCodeValue, int maxK)
            {
                K = PredictKParamMax(K, bitCodeValue, maxK);
            }

            /// Return the positive number of 0-bits in bitstream.
            /// All 0-bits are consumed.
            private uint BitstreamZeros() => _bitStream.ReadUnary1().Value;

            /// Predict K parameter with maximum constraint
            /// Golomb-Rice becomes more efficient when used with an adaptive
            /// K parameter. This is done by predicting the next K value for the
            /// next sample value.
            private int PredictKParamMax(int previousK, uint bitCodeValue, int maxK)
            {
                var newK = previousK;

                if (bitCodeValue >> previousK > 2)
                    newK += 1;
                if (bitCodeValue >> previousK > 5)
                    newK += 1;
                if (bitCodeValue < ((1 << previousK) >> 1))
                    newK -= 1;

                return maxK > 0 ? Math.Min(newK, maxK) : newK;
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
                        //param.LineBuf0 = param.ParamData;
                        //param.LineBuf1 = param.LineBuf0.Slice(0, lineLength);
                        //int* lineBuf = param.LineBuf1 + 1;
                        //if (crxDecodeTopLine(param))
                        //    return -1;
                        //memcpy(bandBuf, lineBuf, param->subbandWidth * sizeof(int32_t));
                        //++param->curLine;
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
            //for (int i = 0; i != buffer.Length; i += 2)
            //{
            //    var a = buffer.Slice(i, 2);
            //    ChangeEndian(a).CopyTo(a);
            //}

            _buffer = buffer;
            _pointer = new BufferPointer();
        }

        private static Memory<byte> ChangeEndian(Memory<byte> buffer)
        {
            Memory<byte> result = new byte[buffer.Length];
            for (int i = 0; i < result.Length; i++)
                result.Span[i] = buffer.Span[result.Length - i - 1];
            return result;
        }

        public bool Read(int bitsCount, out uint value)
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
                value |= (uint)(byteRemaindedValue >> (byteRemainder - readCount));
            }

            return true;
        }

        /// <summary>
        /// Returns count the number of 0 bits in the stream until the next 1 bit.
        /// </summary>
        /// <returns>Amount of 0 bits read.</returns>
        public uint? ReadUnary1()
        {
            // fast realization
            uint counter = 0;
            for (; ; )
            {
                if (!Read(1, out uint value))
                    return null;

                if (value == 1)
                    return counter;
                else
                    counter++;
            }
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
