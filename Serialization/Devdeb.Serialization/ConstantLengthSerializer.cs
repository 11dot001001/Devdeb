using System;

namespace Devdeb.Serialization
{
	public abstract class ConstantLengthSerializer<T> : IConstantLengthSerializer<T>
	{
		private readonly int _size;
		private readonly SerializerFlags _flags;

		protected ConstantLengthSerializer(int size, SerializerFlags flags = SerializerFlags.Empty)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException(nameof(size));
			_size = size;
			_flags = flags;
		}

		public abstract void Serialize(T instance, byte[] buffer, int offset);
		public abstract T Deserialize(byte[] buffer, int offset);

		public int Size => _size;
		public SerializerFlags Flags => _flags;

		public T Deserialize(byte[] buffer, ref int offset)
		{
			try { return Deserialize(buffer, offset); }
			finally { offset += _size; }
		}
		public void Serialize(T instance, byte[] buffer, ref int offset)
		{
			Serialize(instance, buffer, offset);
			offset += _size;
		}

		protected void VerifySerialize(T instance, byte[] buffer, int offset)
		{
			if (!_flags.HasFlag(SerializerFlags.NullInstance) && instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if ((buffer.Length - offset) < Size)
				throw new Exception($"The {nameof(instance)} size exceeds {nameof(buffer.Length)} with {nameof(offset)} : {offset}.");
		}
		protected void VerifyDeserialize(byte[] buffer, int offset)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if ((buffer.Length - offset) < Size)
				throw new Exception($"The {nameof(Size)} exceeds {nameof(buffer.Length)} with {nameof(offset)} : {offset}.");
		}

		T ISerializer<T>.Deserialize(byte[] buffer, ref int offset, int? count)
		{
			if (count != null && count != Size)
				throw new Exception($"{nameof(count)} doesn't match the {nameof(Size)}");
			return Deserialize(buffer, ref offset);
		}
		T ISerializer<T>.Deserialize(byte[] buffer, int offset, int? count)
		{
			if (count != null && count != Size)
				throw new Exception($"{nameof(count)} doesn't match the {nameof(Size)}");
			return Deserialize(buffer, ref offset);
		}
		int ISerializer<T>.Size(T instance) => Size;
	}
}
