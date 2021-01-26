using Devdeb.Serialization;
using Devdeb.Sorage.SorableHeap;

namespace Devdeb.Storage.Serializers
{
	internal class DataSetMetaSerializer : ConstantLengthSerializer<DataSetMeta>
	{
		public DataSetMetaSerializer() : base(StorageSerializers.SegmentSerializer.Size) { }

		public override void Serialize(DataSetMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			StorageSerializers.SegmentSerializer.Serialize(instance.PrimaryIndexesPointer, buffer, offset);
		}
		public override DataSetMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			Segment indexesPointer = StorageSerializers.SegmentSerializer.Deserialize(buffer, offset);
			return new DataSetMeta() { PrimaryIndexesPointer = indexesPointer };
		}
	}
}
