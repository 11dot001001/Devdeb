using Devdeb.Images.CanonRaw.IO;
using System;
using System.Diagnostics;

namespace Devdeb.Images.CanonRaw.Decoding
{
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
                bool wasReaded = _bitStream.Read(escapeBits, out var value);
                Debug.Assert(wasReaded);
                return value;
            }
            else if (K > 0)
            {
                // Golomb-Rice coding : n = q * 2^k + r, with r is next k bits. r is n - (q*2^k)
                bool wasReaded = _bitStream.Read(K, out var value);
                Debug.Assert(wasReaded);
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
}
