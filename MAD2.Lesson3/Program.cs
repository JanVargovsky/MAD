using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD2.Lesson3
{
    class Program
    {
        public class Edge
        {
            public int From { get; set; }
            public int To { get; set; }
        }

        async Task<List<Edge>> ParseCsvAsync(string file)
        {
            var lines = await File.ReadAllLinesAsync(file);
            Edge Parse(string line)
            {
                var tokens = line.Split(';');
                return new Edge
                {
                    From = int.Parse(tokens[0]),
                    To = int.Parse(tokens[1]),
                };
            }
            return lines
                .Select(Parse)
                .ToList();
        }

        List<int> GetNodes(List<Edge> edges)
        {
            var a = edges.Select(t => t.From);
            var b = edges.Select(t => t.To);

            return a.Concat(b)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }


        List<Edge> KernighanLinAlgorithm(List<int> nodes, List<Edge> edges)
        {
            var bestPartitionA = nodes.Take(nodes.Count / 2).ToArray();
            var bestPartitionB = nodes.Skip(nodes.Count / 2).TakeWhile(t => true).ToArray();
            var switchedNodes = new List<int>();

            // calculates number of edges that are between A and B
            int NumberOfEdgesBetweenGroups(int[] a, int[] b, List<Edge> e)
            {
                int result = 0;
                foreach (var edge in e)
                    if (a.Contains(edge.From) && b.Contains(edge.To) ||
                        b.Contains(edge.From) && a.Contains(edge.To))
                        result++;
                return result;
            }


            // number of edges between A nd B
            int bestD = NumberOfEdgesBetweenGroups(bestPartitionA, bestPartitionB, edges);

            while (switchedNodes.Count < nodes.Count)
            {
                // best values for this iteration
                int[] iterationPartitionA = null, iterationPartitionB = null;
                int iterationD = int.MaxValue;
                int nodeIndexA = -1, nodeIndexB = -1;

                for (int i = 0; i < bestPartitionA.Length; i++)
                {
                    if (switchedNodes.Contains(nodes[i]))
                        continue;

                    for (int j = 0; j < bestPartitionB.Length; j++)
                    {
                        if (switchedNodes.Contains(nodes[j]))
                            continue;

                        var partitionA = bestPartitionA.ToArray();
                        var partitionB = bestPartitionB.ToArray();

                        // switch i and j
                        var tmp = partitionA[i];
                        partitionA[i] = partitionB[j];
                        partitionB[j] = tmp;

                        // current number of edges
                        int d = NumberOfEdgesBetweenGroups(partitionA, partitionB, edges);

                        if (d <= iterationD)
                        {
                            iterationPartitionA = partitionA;
                            iterationPartitionB = partitionB;
                            iterationD = d;
                            nodeIndexA = i;
                            nodeIndexB = j;
                        }
                    }
                }

                switchedNodes.Add(bestPartitionA[nodeIndexA]);
                switchedNodes.Add(bestPartitionB[nodeIndexB]);

                if (iterationD < bestD)
                {
                    bestPartitionA = iterationPartitionA;
                    bestPartitionB = iterationPartitionB;
                    bestD = iterationD;
                }
                else
                    break;
            }

            IEnumerable<Edge> GetEdgesWithoutPartitionConnection(int[] a, int[] b, IEnumerable<Edge> e)
            {
                foreach (var edge in e)
                {
                    bool isEdgeAToB = a.Contains(edge.From) && b.Contains(edge.To);
                    bool isEdgeBToA = b.Contains(edge.From) && a.Contains(edge.To);

                    if (!isEdgeAToB && !isEdgeBToA)
                        yield return edge;
                }
            }

            var finalEdges = GetEdgesWithoutPartitionConnection(bestPartitionA, bestPartitionB, edges).ToList();
            return finalEdges;
        }

        async Task ExportToCsvAsync(IEnumerable<Edge> edges, string filename)
        {
            using (var sw = new StreamWriter(filename))
                foreach (var edge in edges)
                    await sw.WriteLineAsync($"{edge.From};{edge.To}");
        }

        static async Task Main(string[] args)
        {
            const string Filename = "../Datasets/KarateClub.csv";
            // Kernighan-Lin
            var p = new Program();
            var edges = await p.ParseCsvAsync(Filename);
            var nodes = p.GetNodes(edges);
            var reducedEdges = p.KernighanLinAlgorithm(nodes, edges);
            await p.ExportToCsvAsync(reducedEdges, "export/kerninghan-lin.csv");
        }
    }
}
