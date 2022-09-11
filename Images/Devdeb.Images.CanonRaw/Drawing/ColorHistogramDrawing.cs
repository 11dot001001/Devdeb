using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;

namespace Devdeb.Images.CanonRaw.Drawing
{
    public class ColorHistogramDrawing
    {
        public const int ColumnWidth = 3;
        public const int ColorColumnHeight = 256;
        public const int ColorChannelHeight = ColorColumnHeight + ColorChannelBorder * 2;
        public const int ColorChannelBorder = 2;

        private static int GetColorChannelWidth(int rangeDepth) => rangeDepth * ColumnWidth + ColorChannelBorder * 2;

        private static (int width, int height) GetBitmapSize(int channelsCount, int rangeDepth)
        {
            int width = GetColorChannelWidth(rangeDepth);
            int height = channelsCount * ColorChannelHeight;

            return (width, height);
        }

        public static void DrawColorChannels(Bitmap bitmap, int[] red, int[] green, int[] blue, int[] brightness)
        {
            if (red.Length != green.Length || red.Length != blue.Length)
                throw new ArgumentException("Different channels range depth");

            (int width, int height) = GetBitmapSize(4, red.Length);
            if (bitmap.Width < width || bitmap.Height < height)
                throw new ArgumentException("Bitmap size small for draw diagram");

            var widthOffset = bitmap.Width - width;
            int heightIndex = 0;

            DrawColorHistogram(bitmap, widthOffset, heightIndex, red, Color.Red, Color.Gray);
            heightIndex += ColorChannelHeight;
            DrawColorHistogram(bitmap, widthOffset, heightIndex, green, Color.Green, Color.Gray);
            heightIndex += ColorChannelHeight;
            DrawColorHistogram(bitmap, widthOffset, heightIndex, blue, Color.Blue, Color.Gray);
            heightIndex += ColorChannelHeight;
            DrawColorHistogram(bitmap, widthOffset, heightIndex, brightness, Color.White, Color.Gray);
        }

        public static void DrawColorChannels(int[] red, int[] green, int[] blue, int[] brightness)
        {
            (int width, int height) = GetBitmapSize(4, red.Length);

            Bitmap bitmap = new(width, height, PixelFormat.Format24bppRgb);
            DrawColorChannels(bitmap, red, green, blue, brightness);
            bitmap.Save($@"C:\Users\lehac\Desktop\channels.png", ImageFormat.Png);
        }

        public static void DrawColorHistogram(
            Bitmap bitmap,
            int widthOffset,
            int heightOffset,
            int[] brightnessFrequencies,
            Color channelColor,
            Color borderColor
        )
        {
            int maxFrequency = brightnessFrequencies.Max();
            double frequencyFactor = (double)ColorColumnHeight / maxFrequency;

            int channelWidth = GetColorChannelWidth(brightnessFrequencies.Length);
            for (int borderIndex = 0; borderIndex != ColorChannelBorder; borderIndex++)
            {
                //draw vertical
                for (int widthIndex = 0; widthIndex != channelWidth; widthIndex++)
                {
                    //top
                    bitmap.SetPixel(
                        widthOffset + widthIndex,
                        heightOffset + borderIndex,
                        borderColor
                    );
                    //bottom
                    bitmap.SetPixel(
                        widthOffset + widthIndex,
                        heightOffset + (ColorChannelHeight - ColorChannelBorder) + borderIndex,
                        borderColor
                    );
                }

                //draw horizontal
                for (int heightIndex = 0; heightIndex != ColorChannelHeight; heightIndex++)
                {
                    //left
                    bitmap.SetPixel(
                        widthOffset + borderIndex,
                        heightOffset + heightIndex,
                        borderColor
                    );
                    //right
                    bitmap.SetPixel(
                        widthOffset + (channelWidth - ColorChannelBorder) + borderIndex,
                        heightOffset + heightIndex,
                        borderColor
                    );
                }
            }

            widthOffset += ColorChannelBorder;
            heightOffset += ColorChannelBorder;

            for (int brightnessIndex = 0; brightnessIndex != brightnessFrequencies.Length; brightnessIndex++)
            {
                int frequencyRow = (int)(brightnessFrequencies[brightnessIndex] * frequencyFactor);
                for (int heightIndex = ColorColumnHeight - frequencyRow; heightIndex != ColorColumnHeight; heightIndex++)
                {
                    for (int columnWidthOffset = 0; columnWidthOffset != ColumnWidth; columnWidthOffset++)
                        bitmap.SetPixel(
                            widthOffset + brightnessIndex * ColumnWidth + columnWidthOffset,
                            heightOffset + heightIndex,
                            channelColor
                        );
                }
            }
        }
    }
}
