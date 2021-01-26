using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using System.Collections.Generic;

namespace Devdeb.Storage.Serializers
{
	internal class MetaSeriaizer : Serializer<Meta>
	{
		private readonly ArrayLengthSerializer<Meta.DataSetMetaMeta> _dataSetMetaMetaArraySerializer;

		public MetaSeriaizer()
		{
			_dataSetMetaMetaArraySerializer = new ArrayLengthSerializer<Meta.DataSetMetaMeta>
			(
				StorageSerializers.DataSetMetaMetaSerializer
			);
		}

		public override int Size(Meta instance)
		{
			VerifySize(instance);
			return _dataSetMetaMetaArraySerializer.Size(instance.DataSetsMetaPointers.ToArray());
		}
		public override void Serialize(Meta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_dataSetMetaMetaArraySerializer.Serialize(instance.DataSetsMetaPointers.ToArray(), buffer, offset);
		}
		public override Meta Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			Meta.DataSetMetaMeta[] array = _dataSetMetaMetaArraySerializer.Deserialize(buffer, offset);
			return new Meta
			{
				DataSetsMetaPointers = new List<Meta.DataSetMetaMeta>(array)
			};
		}
	}
}
