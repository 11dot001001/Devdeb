using System;

namespace Devdeb.Serialization
{
	public interface ISerializer<T>
	{
		int GetSize(T instance);
		void Serialize(T instance, Span<byte> buffer);
		T Deserialize(ReadOnlySpan<byte> buffer);
	}
}
