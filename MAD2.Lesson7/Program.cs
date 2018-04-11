using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD2.Lesson7
{
    class Program
    {
        Random r = new Random(42);

        async Task<Graph> ParseCsvAsync(string file)
        {
            var lines = await File.ReadAllLinesAsync(file);
            Edge Parse(string line)
            {
                var tokens = line.Split(';');
                return new Edge(int.Parse(tokens[0]), int.Parse(tokens[1]));
            }

            var g = Graph.Empty;
            g.Edges = lines
                .Select(Parse)
                .ToList();
            g.Nodes = g.Edges
                .SelectMany(t => new[] { t.From, t.To })
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            return g;
        }

        Graph RandomGraph(int v, int e)
        {
            Graph g = Graph.Empty;
            g.Nodes.AddRange(Enumerable.Range(0, v));
            for (int i = 0; i < e; i++)
            {
                int from = r.Next(v);
                int to;
                do
                {
                    to = r.Next(v);
                } while (from == to && g.Edges.Any(t => t.From == from && t.To == to || t.From == to && t.To == from));

                g.Edges.Add(new Edge(from, to));
            }
            return g;
        }

        void PrintGraph(Graph g)
        {
            foreach (var node in g.Nodes)
            {
                var edges = new List<int>();
                foreach (var edge in g.Edges)
                {
                    if (edge.From == node) edges.Add(edge.To);
                    else if (edge.To == node) edges.Add(edge.From);
                }
                Console.WriteLine($"Node={node}, Edges=[{string.Join(", ", edges)}]");
            }
        }

        static async Task Main(string[] args)
        {
            const string Filename = "../../../../Datasets/KarateClub.csv";
            //const int V = 10;
            //const int E = 20;
            const int K = 4;

            var p = new Program();
            //var g = p.RandomGraph(V, E);
            var g = await p.ParseCsvAsync(Filename);
            var kCore = new KCore();

            Console.WriteLine("Graph");
            p.PrintGraph(g);

            Console.WriteLine("Separated");
            for (int k = 1; k <= K; k++)
            {
                var core = kCore.Compute(g, k);
                Console.WriteLine($"K={k}, Nodes=[{string.Join(", ", core)}] ({core.Count})");
            }

            Console.WriteLine("All");
            var cores = kCore.ComputeAll(g, K);
            for (int k = 0; k < cores.Length; k++)
                Console.WriteLine($"K={k + 1}, Nodes=[{string.Join(", ", cores[k])}] ({cores[k].Count})");
        }
    }
}
