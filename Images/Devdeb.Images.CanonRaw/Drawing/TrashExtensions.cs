using Devdeb.Images.CanonRaw.FileStructure.Chunks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devdeb.Images.CanonRaw.Drawing
{
    public static class TrashExtensions
    {
        public static void DrawImageBoundaries(Bitmap bitmap, TrackBox crxHdImageTrack)
        {
            var leftOpticalBlackOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.LeftOpticalBlackOffset;
            Vector2 leftOpticalBlackPoint1 = new(leftOpticalBlackOffset.Left, leftOpticalBlackOffset.Top);
            Vector2 leftOpticalBlackPoint2 = new(leftOpticalBlackOffset.Right, leftOpticalBlackOffset.Bottom);
            bitmap.DrawRectangle(leftOpticalBlackPoint1, leftOpticalBlackPoint2, Color.Red);

            var topOpticalBlackOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.TopOpticalBlackOffset;
            Vector2 topOpticalBlackPoint1 = new(topOpticalBlackOffset.Left, topOpticalBlackOffset.Top);
            Vector2 topOpticalBlackPoint2 = new(topOpticalBlackOffset.Right, topOpticalBlackOffset.Bottom);
            bitmap.DrawRectangle(topOpticalBlackPoint1, topOpticalBlackPoint2, Color.Red);

            var cropOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.CropOffset;
            Vector2 cropOffsetPoint1 = new(cropOffset.Left, cropOffset.Top);
            Vector2 cropOffsetPoint2 = new(cropOffset.Right, cropOffset.Bottom);
            bitmap.DrawRectangle(cropOffsetPoint1, cropOffsetPoint2, Color.DarkRed);

            var activeAreaOffset = crxHdImageTrack.SampleTable.Craw.CanonDimensions.BigImage.ActiveAreaOffset;
            Vector2 activeAreaOffsetPoint1 = new(activeAreaOffset.Left, activeAreaOffset.Top);
            Vector2 activeAreaOffsetPoint2 = new(activeAreaOffset.Right, activeAreaOffset.Bottom);
            bitmap.DrawRectangle(activeAreaOffsetPoint1, activeAreaOffsetPoint2, Color.AliceBlue);
        }
    }
}
