using Devdeb.Serialization;
using Devdeb.Sorage.SorableHeap.Serializers;

namespace Devdeb.Storage.Serializers
{
	internal sealed class DataMetaSerializer : ConstantLengthSerializer<DataMeta>
	{
		static DataMetaSerializer() => Default = new DataMetaSerializer();
		public static DataMetaSerializer Default { get; }

		public DataMetaSerializer() : base(SegmentSerializer.Default.Size) { }

		public override void Serialize(DataMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			SegmentSerializer.Default.Serialize(instance.DataPointer, buffer, offset);
		}
		public override DataMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return new DataMeta()
			{
				DataPointer = SegmentSerializer.Default.Deserialize(buffer, offset)
			};
		}
	}
}
