using Devdeb.Serialization;
using Devdeb.Storage.Heap;
using System;

namespace Devdeb.Storage
{
	internal class StoredReference<T>
	{
		private Segment _pointer;

		public StoredReference(Segment pointer, ISerializer<T> serializer) : this(pointer, default, serializer) { }
		public StoredReference(Segment pointer, T value, ISerializer<T> serializer)
		{
			_pointer = pointer;
			Value = value;
			Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		}

		public Segment Pointer
		{
			get => _pointer;
			set { _pointer = value; PointerUpdated?.Invoke(this, value); }
		}
		public T Value { get; set; }
		public ISerializer<T> Serializer { get; set; }

		public event EventHandler<Segment> PointerUpdated;
	}
}
