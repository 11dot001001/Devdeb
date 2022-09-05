using System;
using System.Drawing;

namespace Devdeb.Images.CanonRaw.Drawing
{
    public static class BitmapExtensions
    {
        public static void DrawRectangle(this Bitmap bitmap, Vector2 point1, Vector2 point2, Color color)
        {
            Vector2 direction = point2 - point1;

            Vector2 topLeftPoint = new(Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y));
            int xPixelCount = Math.Abs(direction.X);
            int yPixelCount = Math.Abs(direction.Y);

            if (bitmap.Width < topLeftPoint.X + xPixelCount || bitmap.Height < topLeftPoint.Y + yPixelCount)
                throw new InvalidOperationException("The rectangle is out of bitmap.");

            for (int xOffset = 0; xOffset != xPixelCount; xOffset++)
            {
                bitmap.SetPixel(topLeftPoint.X + xOffset, topLeftPoint.Y, color);
                bitmap.SetPixel(topLeftPoint.X + xOffset, topLeftPoint.Y + yPixelCount, color);
            }

            for (int yOffset = 0; yOffset != yPixelCount; yOffset++)
            {
                bitmap.SetPixel(topLeftPoint.X, topLeftPoint.Y + yOffset, color);
                bitmap.SetPixel(topLeftPoint.X + xPixelCount, topLeftPoint.Y + yOffset, color);
            }
        }
    }
}
