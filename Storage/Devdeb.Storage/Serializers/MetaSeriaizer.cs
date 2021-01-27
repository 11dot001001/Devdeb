using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using System.Collections.Generic;

namespace Devdeb.Storage.Serializers
{
	internal sealed class MetaSeriaizer : Serializer<Meta>
	{
		static MetaSeriaizer() => Default = new MetaSeriaizer();
		public static MetaSeriaizer Default { get; }

		private readonly ArrayLengthSerializer<Meta.DataSetMetaMeta> _dataSetSerializer;
		private readonly ArrayLengthSerializer<Meta.DataMetaMeta> _dataSerializer;

		public MetaSeriaizer()
		{
			_dataSetSerializer = new ArrayLengthSerializer<Meta.DataSetMetaMeta>(DataSetMetaMetaSerializer.Default);
			_dataSerializer = new ArrayLengthSerializer<Meta.DataMetaMeta>(DataMetaMetaSerializer.Default);
		}

		public override int Size(Meta instance)
		{
			VerifySize(instance);
			return _dataSetSerializer.Size(instance.DataSetsMetaPointers.ToArray()) +
				   _dataSerializer.Size(instance.DataMetaPointers.ToArray());
		}
		public override void Serialize(Meta instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_dataSetSerializer.Serialize(instance.DataSetsMetaPointers.ToArray(), buffer, ref offset);
			_dataSerializer.Serialize(instance.DataMetaPointers.ToArray(), buffer, offset);
		}
		public override Meta Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			Meta.DataSetMetaMeta[] dataSetArray = _dataSetSerializer.Deserialize(buffer, ref offset);
			Meta.DataMetaMeta[] dataArray = _dataSerializer.Deserialize(buffer, offset);
			return new Meta
			{
				DataSetsMetaPointers = new List<Meta.DataSetMetaMeta>(dataSetArray),
				DataMetaPointers = new List<Meta.DataMetaMeta>(dataArray)
			};
		}
	}
}
