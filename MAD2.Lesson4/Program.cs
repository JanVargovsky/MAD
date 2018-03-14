using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD2.Lesson4
{
    class Program
    {
        readonly Random r = new Random();

        Graph CompleteGraph(int nodes)
        {
            var g = Graph.Empty;

            for (int i = 0; i < nodes; i++)
            {
                g.Nodes.Add(i);
                for (int j = 0; j < nodes; j++)
                    if (i != j)
                        g.Edges.Add(new Edge(i, j));
            }
            return g;
        }

        // m - degree of new node
        // t - count of new nodes
        Graph BarabasiAlbert(Graph g, int m, int t)
        {
            var result = Graph.Empty;
            int id = g.Nodes.Max() + 1;
            var nodes = g.Edges.SelectMany(e => new[] { e.From, e.To }).ToList();

            while (t-- > 0)
            {
                HashSet<int> nodeIdsFrom = new HashSet<int>();
                while (nodeIdsFrom.Count < m)
                {
                    var index = nodes[r.Next(nodes.Count)];
                    nodeIdsFrom.Add(index);
                }

                foreach (var nodeId in nodeIdsFrom)
                {
                    nodes.Add(nodeId);
                    result.Edges.Add(new Edge(nodeId, id));
                }
                result.Nodes.Add(id);
                nodes.Add(id);
                id++;
            }
            return result;
        }

        async Task ExportGraphToCSVAsync(Graph g, string filename)
        {
            using (var sw = new StreamWriter(filename))
                foreach (var edge in g.Edges)
                    await sw.WriteLineAsync($"{edge.From};{edge.To}");
        }

        static async Task Main(string[] args)
        {
            const int N0 = 10; // inital state
            const int M = 3; // each new node has 3 edges
            const int N = 5500; // 5500 nodes

            const double P = 0.15d; // probability of being added
            const int L = 10; // distance in snowball sampling
            const int SnowballCount = 1;

            var p = new Program();
            var completeGraph = p.CompleteGraph(N0);
            await p.ExportGraphToCSVAsync(completeGraph, $"export/K_{N0}.csv");

            var g = p.BarabasiAlbert(completeGraph, M, N);
            await p.ExportGraphToCSVAsync(g, $"export/BA_{N0}_{M}_{N}.csv");

            var sampling = new Sampling();

            var randomNodeSampling = sampling.RandomNode(g, P);
            await p.ExportGraphToCSVAsync(randomNodeSampling, $"export/RandomNode_{P:n2}.csv");

            var randomEdgeSampling = sampling.RandomEdge(g, P);
            await p.ExportGraphToCSVAsync(randomEdgeSampling, $"export/RandomEdge_{P:n2}.csv");

            //var snowballSampling = sampling.Snowball(g, L, SnowballCount);
            //await p.ExportGraphToCSVAsync(snowballSampling, $"export/Snowball_{L}_{SnowballCount}.csv");
        }
    }
}
