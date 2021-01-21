namespace Devdeb.Serialization
{
	public interface IConstantLengthSerializer<T> : ISerializer<T>
	{
		new int Size { get; }
		T Deserialize(byte[] buffer, int offset);
		T Deserialize(byte[] buffer, ref int offset);
	}
}
