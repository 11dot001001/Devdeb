using System;

namespace Devdeb.Sets.Ratios
{
	public struct SurjectionRatio<TInput, TOutput> where TInput : IEquatable<TInput>
	{
		private readonly TInput _input;
		private readonly TOutput _output;

		public SurjectionRatio(TInput input, TOutput output)
		{
			_input = input;
			_output = output;
		}

		public TInput Input => _input;
		public TOutput Output => _output;

		static public bool operator ==(SurjectionRatio<TInput, TOutput> value1, SurjectionRatio<TInput, TOutput> value2)
		{
			return value1._input.Equals(value2._input);
		}
		static public bool operator !=(SurjectionRatio<TInput, TOutput> value1, SurjectionRatio<TInput, TOutput> value2)
		{
			return !(value1 == value2);
		}
	}
}
