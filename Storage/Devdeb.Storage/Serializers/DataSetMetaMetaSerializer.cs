using Devdeb.Serialization;
using Devdeb.Sorage.SorableHeap;

namespace Devdeb.Storage.Serializers
{
	internal class DataSetMetaMetaSerializer : ConstantLengthSerializer<Meta.DataSetMetaMeta>
	{
		static private readonly int _size = StorageSerializers.Int32Serializer.Size + StorageSerializers.SegmentSerializer.Size;

		public DataSetMetaMetaSerializer() : base(_size) { }

		public override void Serialize(Meta.DataSetMetaMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			StorageSerializers.Int32Serializer.Serialize(instance.Id, buffer, ref offset);
			StorageSerializers.SegmentSerializer.Serialize(instance.DataSetMetaPointer, buffer, offset);
		}
		public override Meta.DataSetMetaMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			int id = StorageSerializers.Int32Serializer.Deserialize(buffer, ref offset);
			Segment dataSetMetaPointer = StorageSerializers.SegmentSerializer.Deserialize(buffer, offset);
			return new Meta.DataSetMetaMeta
			{
				Id = id,
				DataSetMetaPointer = dataSetMetaPointer
			};
		}
	}
}
