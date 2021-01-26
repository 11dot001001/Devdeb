using Devdeb.Serialization;
using Devdeb.Serialization.Serializers;
using Devdeb.Serialization.Serializers.System;
using Devdeb.Sets.Generic;
using Devdeb.Sets.Ratios;
using Devdeb.Sorage.SorableHeap;
using System.Linq;

namespace Devdeb.Storage.Serializers
{
	internal class IndexesSerializer : Serializer<RedBlackTreeSurjection<int, Segment>>
	{
		private readonly ArrayLengthSerializer<SurjectionRatio<int, Segment>> _arraySerializer;

		public IndexesSerializer()
		{
			SurjectionRatioSerializer<int, Segment> indexSerializer = new SurjectionRatioSerializer<int, Segment>
			(
				new Int32Serializer(),
				StorageSerializers.SegmentSerializer
			);
			_arraySerializer = new ArrayLengthSerializer<SurjectionRatio<int, Segment>>(indexSerializer);
		}

		public override int Size(RedBlackTreeSurjection<int, Segment> instance)
		{
			VerifySize(instance);
			return _arraySerializer.Size(instance.ToArray());
		}
		public override void Serialize(RedBlackTreeSurjection<int, Segment> instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_arraySerializer.Serialize(instance.ToArray(), buffer, offset);
		}
		public override RedBlackTreeSurjection<int, Segment> Deserialize(byte[] buffer, int offset, int? count = null)
		{
			VerifyDeserialize(buffer, offset, count);
			return new RedBlackTreeSurjection<int, Segment>(_arraySerializer.Deserialize(buffer, offset));
		}
	}
}
