namespace Devdeb.Serialization
{
    public abstract class Serializer<T> : ISerializer<T>
    {
        public abstract void Serialize(T instance, byte[] buffer, ref int index);
        public abstract T Deserialize(byte[] buffer, ref int index);
        public abstract int GetBytesCount(T instance);
    }
}