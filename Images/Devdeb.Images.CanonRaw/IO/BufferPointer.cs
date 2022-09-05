namespace Devdeb.Images.CanonRaw.IO
{
    public struct BufferPointer
    {
        private int _bitOffset;

        public int ByteIndex => _bitOffset / 8;
        public int BitIndex => _bitOffset % 8;
        public int BitOffset => _bitOffset;

        public void AddBits(int count)
        {
            checked { _bitOffset += count; }
        }
        public void SetOffset(int offset) => _bitOffset = offset;
    }
}
