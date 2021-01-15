namespace Devdeb.Storage
{
	public struct Segment
	{
		public long Pointer { get; set; }
		public long Size { get; set; }

		static public bool operator ==(Segment segment1, Segment segment2)
		{
			return segment1.Pointer == segment2.Pointer &&
				   segment1.Size == segment2.Size;
		}
		static public bool operator !=(Segment segment1, Segment segment2)
		{
			return !(segment1 == segment2);
		}
	}
}
