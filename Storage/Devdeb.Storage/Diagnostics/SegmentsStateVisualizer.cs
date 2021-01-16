using Devdeb.Sets.Ratios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Devdeb.Storage.StorableHeap;

namespace Devdeb.Storage.Diagnostics
{
	public class SegmentsStateVisualizer
	{
		public struct SegmentInfo
		{
			public SegmentInfo(Segment segment, bool isUsed)
			{
				Segment = segment;
				IsUsed = isUsed;
			}

			public Segment Segment { get; set; }
			public bool IsUsed { get; }
		}

		private readonly SegmentsInformation _segments;

		internal SegmentsStateVisualizer(SegmentsInformation segments)
		{
			_segments = segments ?? throw new ArgumentNullException(nameof(segments));
		}

		public string GetState()
		{
			string border = "_______________________";
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(border);
			AddUsedSegmentsState(stringBuilder);
			stringBuilder.AppendLine();
			AddFreeSegmentsState(stringBuilder);
			stringBuilder.AppendLine();
			AddFreeSegmentsSizesState(stringBuilder);
			stringBuilder.AppendLine(border);
			return stringBuilder.ToString();
		}
		public string GetConsistentSegments()
		{
			List<SegmentInfo> segmentInfos = new List<SegmentInfo>(_segments.Used.Count + _segments.FreePointers.Count);

			foreach (Segment segment in _segments.Used)
				segmentInfos.Add(new SegmentInfo(segment, true));
			foreach (Segment segment in _segments.FreePointers.Select(x => x.Output))
				segmentInfos.Add(new SegmentInfo(segment, false));

			SegmentInfo[] segments = segmentInfos.OrderBy(x => x.Segment.Pointer).ToArray();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Consistent Segments:");
			AddSegmentsInfo(stringBuilder, segments);
			return stringBuilder.ToString();
		}

		private void AddUsedSegmentsState(StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine("UsedSigments:");
			AddSegmentsInfo(stringBuilder, _segments.Used.ToArray());
		}
		private void AddFreeSegmentsState(StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine("FreeSegments:");
			AddSegmentsInfo(stringBuilder, _segments.FreePointers.Select(x => x.Output).ToArray());
		}
		private void AddFreeSegmentsSizesState(StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine("FreeSegmentsSizes:");
			foreach (SurjectionRatio<long, Queue<Segment>> item in _segments.FreeSizes)
			{
				stringBuilder.AppendLine($"Size {item.Input}:");
				AddSegmentsInfo(stringBuilder, item.Output.ToArray());
				stringBuilder.AppendLine(string.Empty);
			}
		}
		private void AddSegmentInfo(StringBuilder stringBuilder, Segment segment)
		{
			stringBuilder.Append($"Pointer: {segment.Pointer}, Size: {segment.Size}");
		}
		private void AddSegmentInfo(StringBuilder stringBuilder, SegmentInfo segmentInfo)
		{
			stringBuilder.Append("IsUsed: ");
			stringBuilder.Append(segmentInfo.IsUsed ? "Used, " : "Free, ");
			AddSegmentInfo(stringBuilder, segmentInfo.Segment);
		}

		private void AddSegmentsInfo(StringBuilder stringBuilder, Segment[] segments)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				stringBuilder.Append($"{i}. ");
				AddSegmentInfo(stringBuilder, segments[i]);
				stringBuilder.AppendLine(string.Empty);
			}
		}
		private void AddSegmentsInfo(StringBuilder stringBuilder, SegmentInfo[] segments)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				stringBuilder.Append($"{i}. ");
				AddSegmentInfo(stringBuilder, segments[i]);
				stringBuilder.AppendLine(string.Empty);
			}
		}
	}
}
