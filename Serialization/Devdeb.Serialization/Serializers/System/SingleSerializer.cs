namespace Devdeb.Serialization.Serializers.System
{
	public sealed class SingleSerializer : ConstantLengthSerializer<float>
	{
		static SingleSerializer() => Default = new SingleSerializer();
		static public SingleSerializer Default { get; }

		public SingleSerializer() : base(sizeof(float)) { }

		public unsafe override void Serialize(float instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(float*)bufferPointer = instance;
		}
		public unsafe override float Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(float*)bufferPointer;
		}
	}
}
