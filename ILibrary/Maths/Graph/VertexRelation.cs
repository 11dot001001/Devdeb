namespace Devdeb.Maths.Graph
{
    public class VertexRelation
    {
        public GraphVertex RefVertex { get; set; }

        public VertexRelation(GraphVertex refVertex) => RefVertex = refVertex;
    }
    public class VertexRelation<TRelationValue> : VertexRelation
    {
        public TRelationValue Value { get; set; }

        public VertexRelation(GraphVertex refVertex, TRelationValue value) : base(refVertex) => Value = value;
    }
}
