using System.Runtime.InteropServices;

namespace Devdeb.Images.CanonRaw.Drawing
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Pixel42
    {
        [FieldOffset(0)]
        public ushort Red;
        [FieldOffset(2)]
        public ushort Green;
        [FieldOffset(4)]
        public ushort Blue;
    }
}
