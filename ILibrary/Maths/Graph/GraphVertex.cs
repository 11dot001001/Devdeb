using System.Collections;
using System.Collections.Generic;

namespace ILibrary.Maths.Graph
{
    public abstract class GraphVertex : IEnumerable
    {
        internal Dictionary<GraphVertex, VertexRelation> _relations = new Dictionary<GraphVertex, VertexRelation>();

        public IEnumerator GetEnumerator() => _relations.GetEnumerator();
    }

    public class GraphVertex<TValue> : GraphVertex
    {
        public TValue Value { get; set; }

        public GraphVertex(TValue value) => Value = value;

        public override string ToString() => Value.ToString();
    }
}