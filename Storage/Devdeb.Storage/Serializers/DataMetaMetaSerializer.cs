using Devdeb.Serialization;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Storage.Heap.Serializers;

namespace Devdeb.Storage.Serializers
{
	internal sealed class DataMetaMetaSerializer : ConstantLengthSerializer<Meta.DataMetaMeta>
	{
		static DataMetaMetaSerializer() => Default = new DataMetaMetaSerializer();
		public static DataMetaMetaSerializer Default { get; }

		public DataMetaMetaSerializer() : base(Int32Serializer.Default.Size + SegmentSerializer.Default.Size) { }

		public override void Serialize(Meta.DataMetaMeta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			Int32Serializer.Default.Serialize(instance.Id, buffer, ref offset);
			SegmentSerializer.Default.Serialize(instance.DataMetaPointer, buffer, offset);
		}
		public override Meta.DataMetaMeta Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			return new Meta.DataMetaMeta
			{
				Id = Int32Serializer.Default.Deserialize(buffer, ref offset),
				DataMetaPointer = SegmentSerializer.Default.Deserialize(buffer, offset)
			};
		}
	}
}
