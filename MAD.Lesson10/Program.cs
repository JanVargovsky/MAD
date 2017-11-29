using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson10
{
    class Program
    {
        public class Edge
        {
            public Edge(int from, int to)
            {
                From = from;
                To = to;
            }

            public int From { get; set; }
            public int To { get; set; }
        }

        public class Node
        {
            public int Id { get; set; }
        }

        readonly Random r = new Random();

        // Barabasi a Albertova
        List<Edge> BA(IEnumerable<Edge> edges, int m, int t)
        {
            var nodes = edges.SelectMany(e => new[] { e.From, e.To }).ToList();
            int id = nodes.Max() + 1;
            var result = new List<Edge>();

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
                    result.Add(new Edge(nodeId, id));
                }
                nodes.Add(id);
                id++;
            }

            return result;
        }

        IEnumerable<Edge> CompleteGraph(int nodes)
        {
            for (int i = 1; i <= nodes; i++)
                for (int j = i + 1; j <= nodes; j++)
                    yield return new Edge(i, j);
        }

        Task ExportAsync(IEnumerable<Edge> data, string file)
        {
            return File.WriteAllLinesAsync(file, data.Select(t => $"{t.From},{t.To}"));
        }

        int[,] EdgesToMatrix(IEnumerable<Edge> edges)
        {
            int max = edges.Max(t => t.To) + 1 ;
            var matrix = new int[max, max];

            foreach (var e in edges)
                matrix[e.From, e.To] = matrix[e.To, e.From] = 1;

            return matrix;
        }

        static async Task Main(string[] args)
        {
            var p = new Program();
            var k = p.CompleteGraph(4).ToList();
            var g = p.BA(k, 3, 100);
            await p.ExportAsync(g, "ba.csv");
            var m = p.EdgesToMatrix(g);
            
            new Lesson2.Program().WriteAll(m);
            new Lesson3.Program().WriteAll(m);
            new Lesson4.Program().WriteAll(m);
        }
    }
}
