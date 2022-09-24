namespace Devdeb.Serialization
{
	public interface IConstantLengthSerializer<T> : ISerializer<T>
	{
		int Size { get; }

		int ISerializer<T>.GetSize(T instance) => Size;
	}
}
