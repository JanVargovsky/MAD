using System;
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

        public double Modularity<T>(Matrix<T> adjacencyMatrix, Func<T, int> getWeight, int[] classes = null)
        {
            int SumOfEdges(int i) => Enumerable.Range(0, adjacencyMatrix.Size)
                .Sum(j => getWeight(adjacencyMatrix.GetRaw(i, j)));

            var k = Enumerable.Range(0, adjacencyMatrix.Size)
                .Select(SumOfEdges)
                .ToArray();

            int Delta(int i, int j) => classes == null ? 1 :
                classes[i].Equals(classes[j]) ? 1 : 0;

            var m = adjacencyMatrix.Sum(getWeight);

            m /= 2; // every edge is counted twice

            double sum = 0;
            for (int i = 0; i < adjacencyMatrix.Size; i++)
            {
                var ki = k[i];
                for (int j = 0; j < adjacencyMatrix.Size; j++)
                {
                    var A = getWeight(adjacencyMatrix.GetRaw(i, j));
                    var kj = k[j];
                    var f = ki * kj / (2d * m);
                    var delta = Delta(i, j);
                    sum += (A - f) * delta;
                }
            }

            var Q = 1d / (2d * m) * sum;
            return Q;
        }

        public double Modularity(SparseMatrix<Edge> matrix, int[] classes = null)
        {
            int SumOfEdges(int i) => matrix.Get(i).Sum(t => t.Weight);

            var k = Enumerable.Range(0, matrix.Size)
                .Select(SumOfEdges)
                .ToArray();

            int Delta(int i, int j) => classes == null ? 1 :
                classes[i].Equals(classes[j]) ? 1 : 0;

            var m = matrix.Sum(t => t.Weight);

            m /= 2; // every edge is counted twice

            double sum = 0;
            for (int i = 0; i < matrix.Size; i++)
            {
                var ki = k[i];
                for (int j = 0; j < matrix.Size; j++)
                {
                    var edge = matrix.Get(i).FirstOrDefault(t => t.NodeTo == j);
                    var A = edge?.Weight ?? 0;
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
        public List<List<int>> CommunityDetection(SparseMatrix<Edge> matrix, ICollection<int> nodes)
        {
            //var communities = nodes.Select(node => new List<int> { node }).ToList();
            var communities = Enumerable.Range(0, nodes.Count).Select(node => new List<int> { node }).ToList();
            var classes = Enumerable.Range(0, nodes.Count).ToArray(); // unique class for each node

            var m = matrix.Sum(t => t.Weight);

            IEnumerable<(int nodeId, int weight)> GetNeighbors(int i) =>
                matrix.Get(i)
                .Select(t => (t.NodeTo, t.Weight));

            double DeltaQ(int i, int c)
            {
                var sum_in = 0d;
                var sum_tot = 0d;
                var k_i = 0d;
                var k_i_in = 0d;

                // sum_in   = sum of the weights of the links inside C
                // sum_tot  = sum of the weights of the links incident to nodes in C
                // k_i      = sum of the weights of the links incident to node i
                // k_i_in   = sum of the weights of the links from i to nodes in C

                foreach (var node in communities[c])
                {
                    foreach (var (neighbor, weight) in GetNeighbors(node))
                    {
                        if (communities[c].Contains(neighbor))
                            sum_in += weight;
                        else
                            sum_tot += weight;

                        k_i += weight;
                    }
                }

                k_i_in = matrix.Get(i).Where(t => communities[c].Contains(t.NodeTo)).Sum(t => t.Weight);

                var a1 = (sum_in + 2 * k_i_in) / (2 * m);
                var a2 = (sum_tot + k_i) / (2 * m);
                var a = a1 - a2 * a2;

                var b1 = sum_in / (2 * m);
                var b2 = sum_tot / (2 * m);
                var b3 = k_i / (2 * m);
                var b = b1 - b2 * b2 - b3 * b3;

                var q = a - b;
                return q;
            }

            bool anyMove = false;

            foreach (var i in nodes)
            {
                var qs = new List<(double Q, double C)>();

                var maxDeltaQ = double.MinValue;
                var bestC = -1;

                foreach (var j in matrix.Get(i).Select(t => t.NodeTo))
                {
                    //if (classes[i] == classes[j]) continue;

                    //// try remove i from its community and add it to community j
                    //var iOriginalCLass = classes[i]; // original class of i
                    //communities[iOriginalCLass].Remove(i);

                    //classes[i] = classes[j]; // add it to its new one
                    //communities[classes[j]].Add(i);

                    var deltaQ = DeltaQ(i, classes[j]);
                    if (deltaQ > maxDeltaQ)
                    {
                        maxDeltaQ = deltaQ;
                        bestC = classes[j];
                    }

                    qs.Add((deltaQ, classes[j]));

                    // restore back
                    //communities[iOriginalCLass].Add(i);
                    //classes[i] = iOriginalCLass;
                    //communities[classes[j]].Remove(i);
                }

                // positive move
                if (maxDeltaQ > 0 && bestC != -1)
                {
                    var c = classes[i];
                    communities[c].Remove(i);
                    communities[bestC].Add(i);
                    classes[i] = bestC;
                }

                // store also where should i move with deltaQ
            }

            return communities;
        }
    }
}
