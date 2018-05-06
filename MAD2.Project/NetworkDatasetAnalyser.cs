using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD2.Project
{
    public class NetworkDatasetAnalyser
    {
        public double AverageDegree(IEnumerable<Edge> edges, IReadOnlyDictionary<int, Node> nodes)
        {
            if (!edges.Any() || !nodes.Any())
                return 0d;

            var degrees = nodes.ToDictionary(t => t.Key, _ => 0);

            foreach (var edge in edges)
            {
                degrees[edge.NodeFrom]++;
                degrees[edge.NodeTo]++;
            }
            return degrees.Values.Sum() / 2d / degrees.Count;
        }

        public double AverageWeightedDegree(IEnumerable<Edge> edges, IReadOnlyDictionary<int, Node> nodes)
        {
            if (!edges.Any() || !nodes.Any())
                return 0d;

            var degrees = nodes.ToDictionary(t => t.Key, _ => 0);

            foreach (var edge in edges)
            {
                degrees[edge.NodeFrom] += edge.Weight;
                degrees[edge.NodeTo] += edge.Weight;
            }
            return degrees.Values.Sum() / 2d / degrees.Count;
        }

        public List<int>[] WeaklyConnectedComponents(IEnumerable<Edge> inputEdges, IEnumerable<int> inputNodes)
        {
            var edges = new LinkedList<Edge>(inputEdges);
            var visited = new HashSet<int>();

            var components = new List<List<int>>();

            bool AddNodeToComponent(int n, List<int> component)
            {
                bool firstTimeVisited = visited.Add(n);
                if (firstTimeVisited)
                    component.Add(n);
                return firstTimeVisited;
            }

            int? GetFirstAvailableNode()
            {
                var edge = edges.FirstOrDefault(t => !visited.Contains(t.NodeFrom));
                return edge?.NodeFrom;
            }

            int? node;
            while ((node = GetFirstAvailableNode()) != null)
            {
                var component = new List<int>();
                var toVisit = new Queue<int>();
                toVisit.Enqueue(node.Value);

                while (toVisit.Count > 0)
                {
                    var current = toVisit.Dequeue();
                    AddNodeToComponent(current, component);

                    var visitedEdges = new List<LinkedListNode<Edge>>();

                    for (var edgeNode = edges.First; edgeNode != null; edgeNode = edgeNode.Next)
                    {
                        var edge = edgeNode.Value;
                        if (edge.NodeFrom == current)
                        {
                            if (AddNodeToComponent(edge.NodeTo, component))
                                toVisit.Enqueue(edge.NodeTo);
                            visitedEdges.Add(edgeNode);
                        }
                        else if (edge.NodeTo == current)
                        {
                            if (AddNodeToComponent(edge.NodeFrom, component))
                                toVisit.Enqueue(edge.NodeFrom);
                            visitedEdges.Add(edgeNode);
                        }
                    }

                    foreach (var visitedEdge in visitedEdges)
                        edges.Remove(visitedEdge);
                }
                components.Add(component);
            }

            return components.ToArray();
        }

        public double Modularity(Matrix<Edge> adjacencyMatrix, List<Edge> edges)
        {
            int Degree(int i) => Enumerable.Range(0, adjacencyMatrix.Size)
                .Count(j => adjacencyMatrix.GetRaw(i, j) != null);

            var degrees = Enumerable.Range(0, adjacencyMatrix.Size)
                .Select(Degree)
                .ToArray();

            var m = edges.Count;

            double sum = 0;
            for (int i = 0; i < adjacencyMatrix.Size; i++)
            {
                var ki = degrees[i];
                for (int j = 0; j < adjacencyMatrix.Size; j++)
                {
                    var A = adjacencyMatrix.GetRaw(i, j)?.Weight ?? 0;

                    var kj = degrees[j];
                    //var delta = KroneckerDelta(nodes, i, j);
                    var delta = 1d;
                    var k = ki * kj;
                    sum += ((A - (k / (2d * m))) * delta);
                }
            }

            var Q = 1d / (2d * m) * sum;
            return Q;
        }
    }
}
