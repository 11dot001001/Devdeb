using System;
using System.Diagnostics;

namespace Devdeb.Serialization
{
	public abstract class Serializer<T> : ISerializer<T>
	{
		private readonly SerializerFlags _flags;

		protected Serializer(SerializerFlags flags = SerializerFlags.Empty) => _flags = flags;

		public abstract int Size(T instance);
		public abstract T Deserialize(byte[] buffer, int offset, int? count = null);
		public abstract void Serialize(T instance, byte[] buffer, int offset);

		public T Deserialize(byte[] buffer, ref int offset, int? count = null)
		{
			T instance = Deserialize(buffer, offset, count);
			offset += Size(instance);
			return instance;
		}
		public void Serialize(T instance, byte[] buffer, ref int offset)
		{
			Serialize(instance, buffer, offset);
			offset += Size(instance);
		}

		protected void VerifySize(T instance)
		{
			if (!_flags.HasFlag(SerializerFlags.NullInstance) && instance == null)
				throw new ArgumentNullException(nameof(instance));
		}
		protected void VerifySerialize(T instance, byte[] buffer, int offset)
		{
			if (!_flags.HasFlag(SerializerFlags.NullInstance) && instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));
			Debug.Assert
			(
				(buffer.Length - offset) >= Size(instance),
				$"The {nameof(instance)} size exceeds {nameof(buffer.Length)} with {nameof(offset)} : {offset}."
			);
		}
		protected void VerifyDeserialize(byte[] buffer, int offset, int? count)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (_flags.HasFlag(SerializerFlags.NeedCount))
			{
				if (count == null)
					throw new ArgumentNullException(nameof(count));
				if (count < 0)
					throw new ArgumentOutOfRangeException(nameof(count));
				if ((buffer.Length - offset) < count)
					throw new Exception($"The {nameof(count)} exceeds {nameof(buffer.Length)} with {nameof(offset)} : {offset}.");
			}
		}
	}
}
