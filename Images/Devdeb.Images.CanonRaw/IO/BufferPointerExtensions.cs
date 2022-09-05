using System;

namespace Devdeb.Images.CanonRaw.IO
{
    public static class BufferPointerExtensions
    {
        public static bool DoesExceedByteThreshold(this BufferPointer bufferPointer, int threshold)
        {
            return bufferPointer.ByteIndex > threshold;
        }
        public static bool DoesExceedThreshold(this BufferPointer bufferPointer, Memory<byte> buffer)
        {
            return bufferPointer.DoesExceedByteThreshold(buffer.Length);
        }
    }
}
