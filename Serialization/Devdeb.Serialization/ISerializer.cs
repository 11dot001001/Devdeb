namespace Devdeb.Serialization
{
	public interface ISerializer<T>
	{
		SerializerFlags Flags { get; }
		int Size(T instance);
		void Serialize(T instance, byte[] buffer, int offset);
		void Serialize(T instance, byte[] buffer, ref int offset);
		T Deserialize(byte[] buffer, int offset, int? count = null);
		T Deserialize(byte[] buffer, ref int offset, int? count = null);
	}
}
