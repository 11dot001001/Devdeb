using Devdeb.Serialization.Serializers.System;
using Devdeb.Sorage.SorableHeap.Serializers;

namespace Devdeb.Storage.Serializers
{
	static internal class StorageSerializers
	{
		static StorageSerializers()
		{
			Int32Serializer = new Int32Serializer();
			SegmentSerializer = new SegmentSerializer();
			IndexesSerializer = new IndexesSerializer();
			DataSetMetaMetaSerializer = new DataSetMetaMetaSerializer();
			DataSetMetaSerializer = new DataSetMetaSerializer();
			MetaSeriaizer = new MetaSeriaizer();
		}

		static internal Int32Serializer Int32Serializer { get; }
		static internal SegmentSerializer SegmentSerializer { get; }
		static internal IndexesSerializer IndexesSerializer { get; }
		static internal DataSetMetaMetaSerializer DataSetMetaMetaSerializer { get; }
		static internal DataSetMetaSerializer DataSetMetaSerializer { get; }
		static internal MetaSeriaizer MetaSeriaizer { get; }
	}
}
