using System;

namespace Devdeb.Images.CanonRaw.Drawing
{
    public class PixelConverter
    {
        private readonly int _maxColorValue;

        public double BlackPointFactor { get; set; }
        public double WhitePointFactor { get; set; } = 1D;

        public int BlackPoint => (int)(_maxColorValue * BlackPointFactor);
        public int WhitePoint => (int)(_maxColorValue * WhitePointFactor);
        public int ColorPerChannelDepth => WhitePoint - BlackPoint;
        public int ColorQuantization256 => ColorPerChannelDepth / 256;

        public PixelConverter(int maxColorValue)
        {
            _maxColorValue = maxColorValue;
        }

        public Pixel24 ConverPixels(Pixel42 pixel)
        {
            uint red = pixel.Red - (uint)BlackPoint;
            uint green = pixel.Green - (uint)BlackPoint;
            uint blue = pixel.Blue - (uint)BlackPoint;

            red = Math.Max(0, red);
            green = Math.Max(0, green);
            blue = Math.Max(0, blue);

            red = Math.Min((uint)ColorPerChannelDepth, red);
            green = Math.Min((uint)ColorPerChannelDepth, green);
            blue = Math.Min((uint)ColorPerChannelDepth, blue);

            var redValue = ((double)red / ColorPerChannelDepth) * 255;
            var greenValue = ((double)green / ColorPerChannelDepth) * 255;
            var blueValue = ((double)blue / ColorPerChannelDepth) * 255;


            return new Pixel24
            {
                Red = (byte)redValue,
                Green = (byte)greenValue,
                Blue = (byte)blueValue
            };
        }
    }
}
