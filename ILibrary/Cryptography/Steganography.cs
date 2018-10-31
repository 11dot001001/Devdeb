using System;
using System.Drawing;

namespace ILibrary.Cryptography
{
    public class Steganography
    {
        public static class BlindHide
        {
            private static void Write(byte[] bytes, ref OffsetController offsetController, ref Bitmap bitmap)
            {
                if (offsetController.GetFreeBytes(2) < bytes.Length)
                    throw new Exception("Not the free place");

                byte[] sharedBytes = DivideBytes(bytes);

                for (int i = 0; i < sharedBytes.Length; i++)
                {
                    Offset offset = offsetController.GetOffset();
                    bitmap.SetPixel(offset.Horizontal, offset.Vertical, GetNewColor(sharedBytes[i], bitmap.GetPixel(offset.Horizontal, offset.Vertical), offset.Rgb));
                    offsetController.Index++;
                }
            }
            private static byte[] Read(int length, ref OffsetController offsetController, Bitmap bitmap)
            {
                byte[] result = new byte[length];
                byte[] sharedBytes = new byte[length * 4];

                for (int i = 0; i < sharedBytes.Length; i++)
                {
                    Offset offset = offsetController.GetOffset();
                    sharedBytes[i] = GetByteByColor(bitmap.GetPixel(offsetController.Horizontal, offsetController.Vertical), offset.Rgb);
                    offsetController.Index++;
                }
                result = AssembleBytes(sharedBytes);

                return result;
            }

            private static byte[] DivideBytes(byte[] bytes)
            {
                byte[] result = new byte[bytes.Length * 4];
                int offset = 0;

                for (int i = 0; i < bytes.Length; i++)
                {
                    result[offset + 0] = (byte)((bytes[i] >> 3 * 2) & 0b00000011);
                    result[offset + 1] = (byte)((bytes[i] >> 2 * 2) & 0b00000011);
                    result[offset + 2] = (byte)((bytes[i] >> 1 * 2) & 0b00000011);
                    result[offset + 3] = (byte)((bytes[i] >> 0 * 2) & 0b00000011);
                    offset += 4;
                }

                return result;
            }
            private static byte[] AssembleBytes(byte[] sharedBytes)
            {
                byte[] result = new byte[sharedBytes.Length / 4];
                int sharedBytesOffset = 0;

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = (byte)(result[i] + (sharedBytes[sharedBytesOffset++] & 0b00000011));
                    for (int j = 0; j < 3; j++)
                        result[i] = (byte)((result[i] << 2) + (sharedBytes[sharedBytesOffset++] & 0b00000011));
                }

                return result;
            }

            private static Color GetNewColor(byte bytes, Color color, RGB rgb)
            {
                switch (rgb)
                {
                    case RGB.Red: return Color.FromArgb(color.R & 0b11111100 | bytes, color.G, color.B);
                    case RGB.Green: return Color.FromArgb(color.R, color.G & 0b11111100 | bytes, color.B);
                    case RGB.Blue: return Color.FromArgb(color.R, color.G, color.B & 0b11111100 | bytes);
                    default: return color;
                }
            }
            private static byte GetByteByColor(Color color, RGB rgb)
            {
                switch (rgb)
                {
                    case RGB.Red: return color.R;
                    case RGB.Green: return color.G;
                    case RGB.Blue: return color.B;
                    default: return 0;
                }
            }

            public static Bitmap Encrypt(byte[] bytes, Bitmap bitmap)
            {
                OffsetController offsetController = new OffsetController(bitmap.Width, bitmap.Height);
                Bitmap resultBitmap = new Bitmap(bitmap);

                Write(BitConverter.GetBytes(bytes.Length), ref offsetController, ref resultBitmap);
                Write(bytes, ref offsetController, ref resultBitmap);

                return resultBitmap;
            }
            public static byte[] Decrypt(Bitmap bitmap)
            {
                OffsetController offsetController = new OffsetController(bitmap.Width, bitmap.Height);
                int length = BitConverter.ToInt32(Read(4, ref offsetController, bitmap), 0);
                byte[] bytes = new byte[length];
                bytes = Read(bytes.Length, ref offsetController, bitmap);
                return bytes;
            }
        }

        private class OffsetController
        {
            private const int _colorCount = 3;
            private readonly int _maxWidth;
            private readonly int _maxHeight;
            private Offset _currentOffset;

            public int Vertical { get => _currentOffset.Vertical; set => _currentOffset.Vertical = value >= _maxHeight ? throw new Exception() : value; }
            public int Horizontal
            {
                get => _currentOffset.Horizontal;
                set
                {
                    if (value >= _maxWidth)
                    {
                        _currentOffset.Horizontal = 0;
                        _currentOffset.Vertical++;
                    }
                    else
                        _currentOffset.Horizontal = value;
                }
            }
            public RGB Color
            {
                get => _currentOffset.Rgb;
                set
                {
                    if (value > RGB.Blue)
                    {
                        _currentOffset.Rgb = RGB.Red;
                        Horizontal++;
                    }
                    else
                        _currentOffset.Rgb = value;
                }
            }
            public int Index
            {
                get
                {
                    int filledLines = Vertical * _maxWidth * _colorCount;
                    int filledElementsOnLastLine = Horizontal > 0 ? Horizontal * _colorCount : 0;
                    int lastElement = (int)Color;
                    return filledLines + filledElementsOnLastLine + lastElement;
                }
                set
                {
                    Vertical = value / _maxWidth / _colorCount;
                    Horizontal = (value - Vertical * _maxWidth * _colorCount) / _colorCount;
                    Color = (RGB)(value % _colorCount);
                }
            }

            public OffsetController(int maxWidth, int maxHeight) : this(maxWidth, maxHeight, 0, 0, RGB.Red) { }
            public OffsetController(int maxWidth, int maxHeight, int vertical, int horizontal, RGB rgb)
            {
                _maxWidth = maxWidth;
                _maxHeight = maxHeight;
                _currentOffset.Vertical = vertical;
                _currentOffset.Horizontal = horizontal;
                _currentOffset.Rgb = rgb;
            }

            private int GetFreeColors() => ((_maxHeight - 1) - _currentOffset.Vertical) * _maxWidth * _colorCount + ((_maxWidth - 1) - _currentOffset.Horizontal) * _colorCount + _colorCount - (int)Color;

            public int GetFreeBytes(int bitCountInColor) => GetFreeColors() * bitCountInColor / 8;
            public Offset GetOffset() => _currentOffset;
        }

        private struct Offset
        {
            public int Vertical;
            public int Horizontal;
            public RGB Rgb;
        }

        private enum RGB { Red, Green, Blue }
    }
}
