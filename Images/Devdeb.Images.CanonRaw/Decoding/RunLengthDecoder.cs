using System;
using System.Diagnostics;

namespace Devdeb.Images.CanonRaw.Decoding
{
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
                    bool wasReaded = decoder.BitStream.Read(J[sParam], out uint value);
                    Debug.Assert(wasReaded);
                    run_cnt += value;
                }
                sParam = Extensions.SaturatingSub(sParam, 1); // prevent underflow

                if (run_cnt > remaining)
                    throw new Exception("Crx decoder error while decoding line");
            }
            return run_cnt;
        }
    }
}
