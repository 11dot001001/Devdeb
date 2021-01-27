using Devdeb.Serialization;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Sorage.SorableHeap.Serializers;

namespace Devdeb.Storage.Serializers
{
	internal sealed class DataSetMetaMetaSerializer : ConstantLengthSerializer<Meta.DataSetMetaMeta>
	{
		static DataSetMetaMetaSerializer() => Default = new DataSetMetaMetaSerializer();
		public static DataSetMetaMetaSerializer Default { get; }

		public DataSetMetaMetaSerializer() : base(Int32Serializer.Default.Size + SegmentSerializer.Default.Size) { }

		public override void Serialize(Meta.DataSetMetaMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			Int32Serializer.Default.Serialize(instance.Id, buffer, ref offset);
			SegmentSerializer.Default.Serialize(instance.DataSetMetaPointer, buffer, offset);
		}
		public override Meta.DataSetMetaMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return new Meta.DataSetMetaMeta
			{
				Id = Int32Serializer.Default.Deserialize(buffer, ref offset),
				DataSetMetaPointer = SegmentSerializer.Default.Deserialize(buffer, offset)
			};
		}
	}
}
