using MoreLinq;
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

            var m = adjacencyMatrix.Sum(getWeight) / 2;

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

        public double Modularity(SparseMatrix<Edge> matrix, int[] classes)
        {
            int SumOfEdges(int i) => matrix.Get(i).Sum(t => t.Weight);

            var k = Enumerable.Range(0, matrix.Size)
                .Select(SumOfEdges)
                .ToArray();

            int Delta(int i, int j) => classes[i].Equals(classes[j]) ? 1 : 0;

            var m = matrix.Sum(t => t.Weight) / 2;

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
        public Matrix<T> CommunityDetection<T>(Matrix<T> matrix, Func<T, int> getWeight, Func<T, int, T> updateWeight, ICollection<int> nodes)
        {
            //var communities = nodes.Select(node => new List<int> { node }).ToList();
            var communities = Enumerable.Range(0, nodes.Count).Select(node => new List<int> { node }).ToList();
            var classes = Enumerable.Range(0, nodes.Count).ToArray(); // unique class for each node

            var m = matrix.Sum(getWeight) / 2;

            double DeltaQ(int nodeI, int c)
            {
                var sum_in = 0d;
                var sum_tot = 0d;
                var k_i = 0d;
                var k_i_in = 0d;

                // sum_in   = sum of the weights of the links inside C
                // sum_tot  = sum of the weights of the links incident to nodes in C
                // k_i      = sum of the weights of the links incident to node i
                // k_i_in   = sum of the weights of the links from i to nodes in C

                foreach (var i in nodes)
                {
                    foreach (var j in nodes)
                    {
                        if (i == j) continue;
                        var w = getWeight(matrix[i, j]);

                        if (w == 0) continue;

                        bool iInC = communities[c].Contains(i);
                        bool jInC = communities[c].Contains(j);
                        if (iInC && jInC)
                            sum_in += w;

                        if (iInC ^ jInC)
                            sum_tot += w;

                        k_i += w;

                        if ((i == nodeI && jInC) || (j == nodeI && iInC))
                            k_i_in += w;
                    }
                }

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

            while (true)
            {
                var improvement = false;
                // Phase 1
                foreach (var i in nodes)
                {
                    var qs = new List<(double Q, double DeltaQ, double C)>();

                    var qInit = Modularity(matrix, getWeight, classes);
                    var maxDeltaQ = double.NegativeInfinity;
                    var bestC = -1;

                    var neighbors = nodes.Where(k => getWeight(matrix[i, k]) > 0).ToArray();

                    foreach (var j in neighbors)
                    {
                        if (classes[i] == classes[j]) continue;

                        // workaround because DeltaQ doesnt work
                        var backupClass = classes[i];
                        classes[i] = classes[j];
                        var q = Modularity(matrix, getWeight, classes);
                        classes[i] = backupClass;

                        var deltaQ = q - qInit;
                        if (deltaQ > maxDeltaQ)
                        {
                            maxDeltaQ = deltaQ;
                            bestC = classes[j];
                        }

                        qs.Add((deltaQ, q, classes[j]));
                    }

                    // positive move
                    if (maxDeltaQ > 0 && bestC != -1)
                    {
                        var c = classes[i];
                        communities[c].Remove(i);
                        communities[bestC].Add(i);
                        classes[i] = bestC;
                        improvement = true;
                    }
                }

                if (!improvement) break;

                // Phase 2
                var filteredCommunities = communities.Where(t => t.Count > 0).ToArray();
                var newNodes = filteredCommunities.Select((_, i) => i).ToArray();
                var newCommunities = newNodes.Select(node => new List<int> { node }).ToList();
                var newMatrix = new Matrix<T>(newNodes.Length);
                for (int c = 0; c < filteredCommunities.Length; c++)
                    filteredCommunities[c].ForEach(node => classes[node] = c);
                var newClasses = newNodes.ToArray();

                for (int i = 0; i < matrix.Size; i++)
                {
                    var classI = classes[i];
                    for (int j = 0; j < matrix.Size; j++)
                    {
                        var w = getWeight(matrix[i, j]);
                        if (w == 0) continue;
                        var classJ = classes[j];

                        var newWeight = getWeight(newMatrix[classI, classJ]) + w;
                        newMatrix[classI, classJ] = updateWeight(newMatrix[classI, classJ], newWeight);
                        //newMatrix[classI, classJ] += w;

                        // same group -> selfloop edge increase
                        //if (classI == classJ)
                        //{
                        //    newMatrix[classI, classJ] += w;
                        //}
                        //else
                        //{
                        //    newMatrix[classI, classJ] += w;
                        //}
                    }
                }

                //bool anyEmptyRow = Enumerable.Range(0, newMatrix.Size)
                //    .Any(row => Enumerable.Range(0, newMatrix.Size)
                //        .All(col => newMatrix[row, col] == 0));

                nodes = newNodes;
                communities = newCommunities;
                classes = newClasses;
                matrix = newMatrix;
            }

            return matrix;
        }
    }
}
