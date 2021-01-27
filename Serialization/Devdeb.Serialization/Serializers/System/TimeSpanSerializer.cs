using System;

namespace Devdeb.Serialization.Serializers.System
{
	public sealed class TimeSpanSerializer : ConstantLengthSerializer<TimeSpan>
	{
		static TimeSpanSerializer() => Default = new TimeSpanSerializer();
		static public TimeSpanSerializer Default { get; }

		public TimeSpanSerializer() : base(8) { }

		public unsafe override void Serialize(TimeSpan instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(TimeSpan*)bufferPointer = instance;
		}
		public unsafe override TimeSpan Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(TimeSpan*)bufferPointer;
		}
	}
}
