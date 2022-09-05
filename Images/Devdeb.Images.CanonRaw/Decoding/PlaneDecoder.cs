using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using Devdeb.Images.CanonRaw.FileStructure.Image;
using Devdeb.Images.CanonRaw.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace Devdeb.Images.CanonRaw.Decoding
{
    public static class Extensions
    {
        public static uint SaturatingSub(uint a, uint b) => b > a ? 0 : a - b;
    }

    public static class PlaneDecoder
    {
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
                    if (riceDecoder.BitStream.TryRead(1, out uint value) && value == 1)
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
                    if (riceDecoder.BitStream.TryRead(1, out uint value) && value == 1)
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
                    if (riceDecoder.BitStream.TryRead(1, out uint value) && value == 1)
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
                    if (riceDecoder.BitStream.TryRead(1, out uint value) && value == 1)
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

        public static List<ushort[]> DecodePlane(
            Memory<byte> planeMemory,
            TrackBox.SampleTableBox.CrawChunk.CompressionTag hdr,
            TileHeader tileHeader,
            int planeNumber
        )
        {
            Console.WriteLine("start");

            var plane = tileHeader.PlaneHeaders[planeNumber];

            /// |E|Samples........................|E|
            /// |c|bd                           cb|d|
            /// |a|xn                           ax|n|
            ///  ^ ^                               ^
            ///  | |                               |-- Extra sample to provide fake d coefficent
            ///  | |---- First sample value
            ///  |------ Extra sample to provide a fake a/c coefficent

            uint sParam = 0;
            var coefficients = new DecoderCoefficients(1 + hdr.TileWidth + 1);
            var riceDecoder = new RiceDecoder(new ReadOnlyBitStream(planeMemory));
            bool supportsPartial = true;
            int rounded_bits_mask = 0;
            int rounded_bits = 0;

            for (int currentLine = 0; currentLine < hdr.TileHeight; currentLine++)
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

            var median = 1 << (hdr.BitsPerSample - 1);
            var max_val = (1 << hdr.BitsPerSample) - 1;

            var newCoefficients = new List<ushort[]>();
            foreach (int[] planeLine in coefficients.SwappedBuffers)
            {
                ushort[] convertedLine = new ushort[planeLine.Length];

                for (int i = 0; i < planeLine.Length; i++)
                    convertedLine[i] = Constrain(median + planeLine[i], 0, max_val);

                newCoefficients.Add(convertedLine);
            }

            return newCoefficients;
        }

        private static ushort Constrain(int Value, int minValue, int maxValue)
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
    }
}
