using System.Collections.Generic;
using System.Linq;

namespace MAD2.Project
{
    public class NetworkDatasetAnalyser
    {
        // <NodeId, Degree>
        Dictionary<int, int> Degrees(IEnumerable<Edge> edges, IEnumerable<int> nodes)
        {
            var degrees = nodes.ToDictionary(t => t, _ => 0);

            foreach (var edge in edges)
            {
                degrees[edge.NodeFrom]++;
                degrees[edge.NodeTo]++;
            }

            return degrees;
        }

        public double AverageDegree(IEnumerable<Edge> edges, IEnumerable<int> nodes)
        {
            if (!edges.Any() || !nodes.Any())
                return 0d;

            var degrees = Degrees(edges, nodes);
            return degrees.Values.Sum() / 2d / degrees.Count;
        }

        public double AverageWeightedDegree(IEnumerable<Edge> edges, IEnumerable<int> nodes)
        {
            if (!edges.Any() || !nodes.Any())
                return 0d;

            var degrees = nodes.ToDictionary(t => t, _ => 0);

            foreach (var edge in edges)
            {
                degrees[edge.NodeFrom] += edge.Weight;
                degrees[edge.NodeTo] += edge.Weight;
            }
            return degrees.Values.Sum() / 2d / degrees.Count;
        }

        public (int Degree, int Count)[] DegreeHistogram(IEnumerable<Edge> edges, IEnumerable<int> nodes)
        {
            var degrees = Degrees(edges, nodes);

            var min = degrees.Values.Min();
            var max = degrees.Values.Max();
            var result = new(int Degree, int Count)[max - min + 1];

            for (int degree = min; degree <= max; degree++)
                result[degree - min] = (degree, 0);

            foreach (var degree in degrees.Values)
                result[degree - min].Count++;

            return result;
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

        public double Modularity(Matrix<Edge> adjacencyMatrix, int[] classes = null)
        {
            int SumOfEdges(int i) => Enumerable.Range(0, adjacencyMatrix.Size)
                .Sum(j => adjacencyMatrix.GetRaw(i, j)?.Weight ?? 0);

            var k = Enumerable.Range(0, adjacencyMatrix.Size)
                .Select(SumOfEdges)
                .ToArray();

            int Delta(int i, int j) => classes == null ? 1 :
                classes[i].Equals(classes[j]) ? 1 : 0;

            var m = adjacencyMatrix.Where(t => t != null).Sum(t => t.Weight);

            double sum = 0;
            for (int i = 0; i < adjacencyMatrix.Size; i++)
            {
                var ki = k[i];
                for (int j = 0; j < adjacencyMatrix.Size; j++)
                {
                    if (adjacencyMatrix.GetRaw(i, j) == null)
                        continue;
                    var A = adjacencyMatrix.GetRaw(i, j).Weight;
                    var kj = k[j];
                    var f = ki * kj / (2d * m);
                    var delta = Delta(i, j);
                    sum += (A - f) * delta;
                }
            }

            var Q = 1d / (2d * m) * sum;
            return Q;
        }

        // Fast unfolding of communities in large networks https://arxiv.org/abs/0803.0476
        public List<List<int>> CommunityDetection(Matrix<Edge> adjacencyMatrix, ICollection<int> nodes)
        {
            //var communities = nodes.Select(node => new List<int> { node }).ToList();
            var communities = Enumerable.Range(0, nodes.Count).Select(node => new List<int> { node }).ToList();
            var classes = Enumerable.Range(0, nodes.Count).ToArray(); // unique class for each node

            var m = adjacencyMatrix.Where(t => t != null).Sum(t => t.Weight);

            IEnumerable<(int nodeId, double weight)> GetNeighbors(int i)
            {
                for (int j = 0; j < adjacencyMatrix.Size; j++)
                {
                    var edge = adjacencyMatrix.GetRaw(i, j);
                    if (edge?.Weight > 0)
                        yield return (j, edge.Weight);
                    else
                    {
                        edge = adjacencyMatrix.GetRaw(j, i);
                        if (edge?.Weight > 0)
                            yield return (j, edge.Weight);
                    }
                }
            }

            double DeltaQ(int communityIndex)
            {
                var sum_in = 0d;
                var sum_tot = 0d;
                var k_i = 0d;
                var k_i_in = 0d;

                // go through all nodes in the community i and check its edges
                // sum_in   = sum of the weights of the links inside C
                // sum_tot  = sum of the weights of the links incident to nodes in C
                // k_i      = sum of the weights of the links incident to node i
                // k_i_in   = sum of the weights of the links from i to nodes in C
                foreach (var node in communities[communityIndex])
                {
                    foreach (var (neighbor, weight) in GetNeighbors(node))
                    {
                        if (communities[communityIndex].Contains(neighbor))
                            sum_in += weight;
                        else
                            sum_tot += weight;

                        k_i += weight;
                    }
                }

                var a1 = (sum_in + 2 * k_i_in) / (2 * m);
                var a2 = (sum_tot + k_i) / (2 * m);
                var a = a1 - a2 * a2;

                var b1 = sum_in / (2 * m);
                var b2 = sum_tot / (2 * m);
                var b3 = k_i / (2 * m);
                var b = b1 - b2*b2 - b3*b3;

                var q = a - b;
                return q;
            }

            foreach (var i in nodes)
            {
                var maxDeltaQ = -double.MinValue;

                foreach (var j in nodes)
                {
                    if (classes[i] == classes[j]) continue;

                    // try remove i from its community and add it to community j
                    var iOriginalCLass = classes[i]; // original class of i
                    communities[iOriginalCLass].Remove(i);

                    classes[i] = classes[j]; // add it to its new one
                    communities[classes[j]].Add(i);

                    var deltaQ = DeltaQ(classes[j]);
                    if (deltaQ > maxDeltaQ)
                        maxDeltaQ = deltaQ;

                    // restore back
                    communities[iOriginalCLass].Add(i);
                    classes[i] = iOriginalCLass;
                    communities[classes[j]].Remove(i);
                }

                // store also where should i move with deltaQ
            }

            return communities;
        }
    }
}
