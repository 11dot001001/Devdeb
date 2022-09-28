using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Devdeb.Images.CanonRaw.Drawing
{
    public static class PixelsConvert
    {
        private static readonly ParallelOptions _parallelOptions = new()
        { MaxDegreeOfParallelism = Environment.ProcessorCount };
        //{ MaxDegreeOfParallelism = 1 };

        public unsafe static byte[] ToPixel24ByteArray(
            Pixel42[,] pixels,
            Func<Pixel42, Pixel24> convertPixel,
            AreaSize pixelsArea,
            PixelConverter pixelConverter
        )
        {
            byte[] imageBuffer = new byte[pixels.Length * 3];

            var lineColorComponents = pixelsArea.Width * 3;
            const int colorComponentPerProcessingIteration = 16;

            var lineProcessingIterations = Math.DivRem(lineColorComponents, colorComponentPerProcessingIteration, out int remainder);
            if (remainder != 0)
                throw new NotImplementedException("Сalculations with a width remainder");

            var zeroVector = Vector256.Create((short)0);
            var blackPointVector = Vector256.Create(checked((short)pixelConverter.BlackPoint));
            var colorPerChannelDepthVector = Vector256.Create(checked((short)pixelConverter.ColorPerChannelDepth));

            double colorPerChannelFactor = 255D / pixelConverter.ColorPerChannelDepth;
            if (double.IsInfinity(colorPerChannelFactor))
                colorPerChannelFactor = 0;

            var colorPerChannelFactorVector = Vector256.Create((float)colorPerChannelFactor);
            Parallel.For(0, pixelsArea.Height, _parallelOptions, height =>
            {
                Span<int> stack8ColorsResult = stackalloc int[8];

                fixed (Pixel42* linePixelPointer = &pixels[height, 0])
                fixed (byte* lineImageBufferPointer = &imageBuffer[GetByteIndex(height, 0, pixelsArea.Width)])
                {
                    for (int lineProcessingIndex = 0; lineProcessingIndex != lineProcessingIterations; lineProcessingIndex++)
                    {
                        ushort* processingColorPointer = (ushort*)linePixelPointer + lineProcessingIndex * colorComponentPerProcessingIteration;
                        byte* storeColorPointer = (byte*)lineImageBufferPointer + lineProcessingIndex * colorComponentPerProcessingIteration;

                        //16 short
                        var colors = Vector256.Create(
                            *(ulong*)processingColorPointer,
                            *((ulong*)processingColorPointer + 1),
                            *((ulong*)processingColorPointer + 2),
                            *((ulong*)processingColorPointer + 3)
                        ).AsInt16();
                        //var colors = Vector256.Create(
                        //    *processingColorPointer,
                        //    *(processingColorPointer + 1),
                        //    *(processingColorPointer + 2),
                        //    *(processingColorPointer + 3),
                        //    *(processingColorPointer + 4),
                        //    *(processingColorPointer + 5),
                        //    *(processingColorPointer + 6),
                        //    *(processingColorPointer + 7),
                        //    *(processingColorPointer + 8),
                        //    *(processingColorPointer + 9),
                        //    *(processingColorPointer + 10),
                        //    *(processingColorPointer + 11),
                        //    *(processingColorPointer + 12),
                        //    *(processingColorPointer + 13),
                        //    *(processingColorPointer + 14),
                        //    *(processingColorPointer + 15)
                        //).AsInt16();

                        colors = Avx2.Subtract(colors, blackPointVector);
                        colors = Avx2.Max(colors, zeroVector);
                        colors = Avx2.Min(colors, colorPerChannelDepthVector);

                        // | r g b   r g b   r g b   r g b   r g b   r | g b
                        // 16 color components => 32 bytes
                        // Vector256 contains 8 float
                        // 1. Convert 16 color components to 2 parts by 8 floats
                        // 2. 2 parts by 8 floats  mul colorPerChannelFactor
                        // 3. convert 2 parts by 8 floats to 1 part by 16 bytes
                        // 4. save to dist array.

                        Vector256<int> lowerResult = Convert(colors.GetLower());
                        fixed (int* stackIntResultPointer = stack8ColorsResult)
                            Avx.Store(stackIntResultPointer, lowerResult);

                        for (int i = 0; i < stack8ColorsResult.Length; i++)
                            *(storeColorPointer + i) = checked((byte)stack8ColorsResult[i]);

                        Vector256<int> upperResult = Convert(colors.GetUpper());
                        fixed (int* stackIntResultPointer = stack8ColorsResult)
                            Avx.Store(stackIntResultPointer, upperResult);

                        for (int i = 0; i < stack8ColorsResult.Length; i++)
                            *(storeColorPointer + i + 8) = checked((byte)stack8ColorsResult[i]);
                    }
                }

                Vector256<int> Convert(Vector128<short> colors)
                {
                    Vector256<float> colors256 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(colors));
                    Vector256<float> colorsRounded = Avx.RoundToNearestInteger(Avx.Multiply(colors256, colorPerChannelFactorVector));
                    return Avx.ConvertToVector256Int32(colorsRounded);
                }
            });

            return imageBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetByteIndex(int height, int width, int lineLength) => (height * lineLength + width) * 3;
    }
}
