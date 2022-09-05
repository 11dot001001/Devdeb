using System;
using System.IO;
using System.Numerics;

namespace Devdeb.Images.CanonRaw.IO
{
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

        public bool TryRead(int bitsCount, out uint value)
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
            int zeroCounter = 0;
            for (; ; )
            {
                int byteRemainder = 8 - _pointer.BitIndex;
                int byteRemaindedValue = _buffer.Span[_pointer.ByteIndex] & _masks[byteRemainder - 1];
                var leadingZero = BitOperations.LeadingZeroCount((uint)byteRemaindedValue) - 24 - _pointer.BitIndex;

                zeroCounter += leadingZero;
                _pointer.AddBits(leadingZero);

                if (leadingZero == byteRemainder)
                    continue;

                _pointer.AddBits(1);
                return (uint)zeroCounter;
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
}
