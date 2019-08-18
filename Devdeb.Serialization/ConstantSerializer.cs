namespace Devdeb.Serialization
{
    public abstract class ConstantSerializer<T> : Serializer<T>
    {
        static private int _bytesCount;

        static public int BytesCount => _bytesCount;

        protected ConstantSerializer(int bytesCount) => _bytesCount = bytesCount;

        public override int GetBytesCount(T instance) => _bytesCount;
    }
}