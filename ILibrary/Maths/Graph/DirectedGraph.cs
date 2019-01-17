namespace Devdeb.Maths.Graph
{
    public class DirectedGraph<TVertexValue> : Graph
    {
        public void AddVertex(GraphVertex<TVertexValue> graphVertex) => base.AddVertex(graphVertex);
        public void RemoveVertex(GraphVertex<TVertexValue> graphVertex) => base.RemoveVertex(graphVertex);

        public void AddRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2) => vertex1._relations.Add(vertex2, new VertexRelation(vertex2));
        public void RemoveRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2) => vertex1._relations.Remove(vertex2);
    }
    public class DirectedGraph<TVertexValue, TRelationValue> : Graph<TRelationValue>
    {
        public void AddVertex(GraphVertex<TVertexValue> graphVertex) => base.AddVertex(graphVertex);
        public void RemoveVertex(GraphVertex<TVertexValue> graphVertex) => base.RemoveVertex(graphVertex);

        public void AddRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2, TRelationValue relationValue) => vertex1._relations.Add(vertex2, new VertexRelation<TRelationValue>(vertex2, relationValue));
        public void RemoveRelation(GraphVertex<TVertexValue> vertex1, GraphVertex<TVertexValue> vertex2) => vertex1._relations.Remove(vertex2);
    }
}
