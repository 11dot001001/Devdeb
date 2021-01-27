namespace Devdeb.Serialization.Serializers.System
{
	public class CharSerializer : ConstantLengthSerializer<char>
	{
		static CharSerializer() => Default = new CharSerializer();
		static public CharSerializer Default { get; }

		public CharSerializer() : base(sizeof(char)) { }

		public unsafe override void Serialize(char instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				*(float*)bufferPointer = instance;
		}
		public unsafe override char Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			fixed (byte* bufferPointer = &buffer[offset])
				return *(char*)bufferPointer;
		}
	}
}
