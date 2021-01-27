using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Devdeb.Sorage.SorableHeap.Diagnostics
{
	public class StorableHeapDiagnostic
	{
		private readonly StorableHeap _storableHeap;

		public StorableHeapDiagnostic(StorableHeap storableHeap)
		{
			_storableHeap = storableHeap ?? throw new ArgumentNullException(nameof(storableHeap));
		}

		public void EnsureFreePointersAndSizesCompliance()
		{
			List<Segment> pointers = _storableHeap.Segments.FreePointers.Select(x => x.Output).ToList();
			Queue<Segment>[] sizes = _storableHeap.Segments.FreeSizes.Select(x => x.Output).ToArray();

			foreach (Queue<Segment> queue in sizes)
				foreach (Segment segment in queue.ToArray())
					Debug.Assert(pointers.Remove(segment));
			Debug.Assert(pointers.Count == 0);
		}
	}
}
