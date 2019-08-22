namespace Devdeb.Serialization
{
    public interface ISerializer<T>
    {
        int GetBytesCount(T instance);
        void Serialize(T instance, byte[] buffer, ref int index);
		T Deserialize(byte[] buffer, ref int index);
    }
}