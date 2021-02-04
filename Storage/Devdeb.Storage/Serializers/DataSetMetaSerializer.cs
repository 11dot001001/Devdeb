using Devdeb.Serialization;
using Devdeb.Storage.Heap.Serializers;

namespace Devdeb.Storage.Serializers
{
	internal sealed class DataSetMetaSerializer : ConstantLengthSerializer<DataSetMeta>
	{
		static DataSetMetaSerializer() => Default = new DataSetMetaSerializer();
		public static DataSetMetaSerializer Default { get; }

		public DataSetMetaSerializer() : base(SegmentSerializer.Default.Size) { }

		public override void Serialize(DataSetMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			SegmentSerializer.Default.Serialize(instance.PrimaryIndexesPointer, buffer, offset);
		}
		public override DataSetMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return new DataSetMeta() 
			{ 
				PrimaryIndexesPointer = SegmentSerializer.Default.Deserialize(buffer, offset)
			};
		}
	}
}
