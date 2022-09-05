using System;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Images.CanonRaw.Decoding
{
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
}
