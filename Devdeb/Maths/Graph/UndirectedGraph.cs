using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Devdeb.Maths.Graph
{
    public class UndirectedGraph<TVertexValue> : Graph, IEnumerable<GraphVertex<TVertexValue>>
    {
        public void AddVertex(GraphVertex<TVertexValue> graphVertex) =>  base.AddVertex(graphVertex);
        public void RemoveVertex(GraphVertex<TVertexValue> graphVertex) =>base.RemoveVertex(graphVertex);

        public void AddRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2)
        {
            vertex1._relations.Add(vertex2, new VertexRelation(vertex2));
            vertex2._relations.Add(vertex1, new VertexRelation(vertex1));
        }
        public void RemoveRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2)
        {
            _ = vertex1._relations.Remove(vertex2);
            _ = vertex2._relations.Remove(vertex1);
        }

        public IEnumerator<GraphVertex<TVertexValue>> GetEnumerator() => _vertices.Cast<GraphVertex<TVertexValue>>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _vertices.GetEnumerator();
    }

    public class UndirectedGraph<TVertexValue, TRelationValue> : Graph<TRelationValue>, IEnumerable<GraphVertex<TVertexValue>>
    {
        public void AddVertex(GraphVertex<TVertexValue> graphVertex) => base.AddVertex(graphVertex);
        public void RemoveVertex(GraphVertex<TVertexValue> graphVertex) => base.RemoveVertex(graphVertex);

        public void AddRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2, TRelationValue relationValue)
        {
            vertex1._relations.Add(vertex2, new VertexRelation<TRelationValue>(vertex2, relationValue));
            vertex2._relations.Add(vertex1, new VertexRelation<TRelationValue>(vertex1, relationValue));
        }
        public void RemoveRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2)
        {
            _ = vertex1._relations.Remove(vertex2);
            _ = vertex2._relations.Remove(vertex1);
        }

        public IEnumerator<GraphVertex<TVertexValue>> GetEnumerator() => _vertices.Cast<GraphVertex<TVertexValue>>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _vertices.GetEnumerator();
    }
}