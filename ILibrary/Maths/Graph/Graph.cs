using System;
using System.Collections.Generic;
using System.Linq;

namespace ILibrary.Maths.Graph
{
    public abstract class Graph
    {
        internal readonly HashSet<GraphVertex> _vertices = new HashSet<GraphVertex>();

        protected void AddVertex(GraphVertex vertex) => _vertices.Add(vertex);
        protected void RemoveVertex(GraphVertex vertex) => _vertices.Remove(vertex);

        public void Reset() => _vertices.Clear();

        public IEnumerable<GraphVertex> BreadthFirstSearch(GraphVertex startVertex, GraphVertex endVertex)
        {
            List<VertexAlgoritmStruct> listVertex = new List<VertexAlgoritmStruct>(_vertices.Count);
            foreach (GraphVertex vertex in _vertices)
                listVertex.Add(new VertexAlgoritmStruct(vertex));

            VertexAlgoritmStruct start = listVertex.Single(x => x.Vertex == startVertex);
            VertexAlgoritmStruct end = listVertex.Single(x => x.Vertex == endVertex);
            start.IsMarked = true;

            Queue<VertexAlgoritmStruct> queue = new Queue<VertexAlgoritmStruct>();
            queue.Enqueue(start);

            while (queue.Count != 0)
            {
                VertexAlgoritmStruct algoritmVertex = queue.Dequeue();
                foreach (VertexRelation item in algoritmVertex.Vertex._relations.Values)
                {
                    VertexAlgoritmStruct refVertex = listVertex.Single(x => x.Vertex == item.RefVertex);
                    if (refVertex.IsMarked) continue;
                    refVertex.RefVertex = algoritmVertex;
                    refVertex.IsMarked = true;
                    queue.Enqueue(refVertex);
                }
            }

            List<VertexAlgoritmStruct> result = new List<VertexAlgoritmStruct> { end };
            for (; ; )
            {
                if (result[result.Count - 1] == start)
                    break;
                result.Add(result[result.Count - 1].RefVertex);
            }
            return result.Select(x => x.Vertex).Reverse();
        }
    }
    public abstract class Graph<TRelationValue> : Graph
    {
        public int Dijkstra(GraphVertex startVertex, GraphVertex endVertex, out IEnumerable<GraphVertex> path, Func<VertexRelation, int> waitGetter)
        {
            List<VertexAlgoritmStruct> dijkstraVertices = new List<VertexAlgoritmStruct>(_vertices.Count);

            foreach (GraphVertex vertex in _vertices)
                dijkstraVertices.Add(new VertexAlgoritmStruct(vertex));

            //если будет коосяк => вместо First - Single
            VertexAlgoritmStruct start = dijkstraVertices.First(x => x.Vertex == startVertex);
            VertexAlgoritmStruct end = dijkstraVertices.First(x => x.Vertex == endVertex);

            start.PathValue = 0;
            for (; ; )
            {
                if (dijkstraVertices.Count(x => !x.IsMarked) != 0)
                {
                    VertexAlgoritmStruct min = dijkstraVertices.Where(x => !x.IsMarked).First(x => x.PathValue == dijkstraVertices.Where(y => !y.IsMarked).Min(y => y.PathValue));

                    foreach (VertexRelation item in min.Vertex._relations.Values)
                    {
                        VertexAlgoritmStruct vertex = dijkstraVertices.SingleOrDefault(x => x.Vertex == item.RefVertex);
                        int weight = waitGetter(item);
                        if (vertex?.PathValue > weight + min.PathValue)
                        {
                            vertex.PathValue = weight + min.PathValue;
                            vertex.RefVertex = min;
                        }
                    }
                    min.IsMarked = true;
                }
                else break;
            }

            List<VertexAlgoritmStruct> result = new List<VertexAlgoritmStruct> { end };
            for (; ; )
            {
                if (result[result.Count - 1] == start)
                    break;
                result.Add(result[result.Count - 1].RefVertex);
            }
            path = result.Select(x => x.Vertex).Reverse();
            return end.PathValue;
        }
        public IEnumerable<GraphVertex> Dijkstra(GraphVertex startVertex, GraphVertex endVertex, Func<VertexRelation, int> waitGetter)
        {
            //я это писал, когда был долбаёбом. Если я смотрю это сейчас, перепиши код получше, потому что ты красавчик.
            List<VertexAlgoritmStruct> dijkstraVertexs = new List<VertexAlgoritmStruct>(_vertices.Count);

            foreach (GraphVertex vertex in _vertices)
                dijkstraVertexs.Add(new VertexAlgoritmStruct(vertex));

            VertexAlgoritmStruct start = dijkstraVertexs.First(x => x.Vertex == startVertex);
            VertexAlgoritmStruct end = dijkstraVertexs.First(x => x.Vertex == endVertex);

            start.PathValue = 0;
            for (; ; )
            {
                if (dijkstraVertexs.Count(x => !x.IsMarked) != 0)
                {
                    VertexAlgoritmStruct min = dijkstraVertexs.Where(x => !x.IsMarked).First(x => x.PathValue == dijkstraVertexs.Where(y => !y.IsMarked).Min(y => y.PathValue));

                    foreach (VertexRelation item in min.Vertex._relations.Values)
                    {
                        VertexAlgoritmStruct vertex = dijkstraVertexs.SingleOrDefault(x => x.Vertex == item.RefVertex);
                        int weight = waitGetter(item);

                        if (vertex?.PathValue > weight + min.PathValue)
                        {
                            vertex.PathValue = weight + min.PathValue;
                            vertex.RefVertex = min;
                        }
                    }
                    min.IsMarked = true;
                }
                else break;
            }

            List<VertexAlgoritmStruct> result = new List<VertexAlgoritmStruct> { end };
            for (; ; )
            {
                if (result[result.Count - 1] == start)
                    break;
                result.Add(result[result.Count - 1].RefVertex);
            }
            return result.Select(x => x.Vertex).Reverse();
        }
    }

    internal class VertexAlgoritmStruct
    {
        public GraphVertex Vertex;
        public VertexAlgoritmStruct RefVertex;
        public bool IsMarked;
        public int PathValue;

        public VertexAlgoritmStruct(GraphVertex vertex) : this(vertex, int.MaxValue) { }
        public VertexAlgoritmStruct(GraphVertex vertex, int pathValue) : this(vertex, false, pathValue) { }
        public VertexAlgoritmStruct(GraphVertex vertex, bool isMarked, int pathValue)
        {
            Vertex = vertex;
            IsMarked = isMarked;
            PathValue = pathValue;
        }
    }
}