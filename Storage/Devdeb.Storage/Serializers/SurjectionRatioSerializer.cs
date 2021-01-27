using Devdeb.Serialization;
using Devdeb.Sets.Ratios;
using System;

namespace Devdeb.Storage.Serializers
{
	internal sealed class SurjectionRatioSerializer<TInput, TOutput> : ConstantLengthSerializer<SurjectionRatio<TInput, TOutput>>
		where TInput : struct
		where TOutput : struct
	{
		private readonly IConstantLengthSerializer<TInput> _inputSerializer;
		private readonly IConstantLengthSerializer<TOutput> _outputSerializer;

		public SurjectionRatioSerializer
		(
			IConstantLengthSerializer<TInput> inputSerializer,
			IConstantLengthSerializer<TOutput> outputSerializer
		) : base(inputSerializer.Size + outputSerializer.Size)
		{
			_inputSerializer = inputSerializer ?? throw new ArgumentNullException(nameof(inputSerializer));
			_outputSerializer = outputSerializer ?? throw new ArgumentNullException(nameof(outputSerializer));
		}

		public override void Serialize(SurjectionRatio<TInput, TOutput> instance, byte[] buffer, int offset)
		{
			VerifySerialize(instance, buffer, offset);
			_inputSerializer.Serialize(instance.Input, buffer, ref offset);
			_outputSerializer.Serialize(instance.Output, buffer, offset);
		}
		public override SurjectionRatio<TInput, TOutput> Deserialize(byte[] buffer, int offset)
		{
			VerifyDeserialize(buffer, offset);
			TInput input = _inputSerializer.Deserialize(buffer, ref offset);
			TOutput output = _outputSerializer.Deserialize(buffer, offset);
			return new SurjectionRatio<TInput, TOutput>(input, output);
		}
	}
}
