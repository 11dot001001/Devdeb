namespace Devdeb.Serialization
{
    public interface ISerializer<T>
    {
        void Serialize(T instance, byte[] buffer, ref int index);
        T Deserialize(byte[] buffer, ref int index);
        int GetBytesCount(T instance);
    }
}