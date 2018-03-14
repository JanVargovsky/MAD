using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD2.Lesson5
{
    class Program
    {
        Random r = new Random();

        Graph RandomGraph(int n, int edges)
        {
            Graph g = new Graph(n);

            for (int i = 0; i < edges; i++)
            {
                int from = r.Next(n);
                int to;
                do
                {
                    to = r.Next(n);
                } while (from == to);
                g[from, to] = true;
            }

            // je souvisly?

            return g;
        }

        // n = number of new nodes
        // p = probability
        Graph Generate(Graph initial, int n, float p)
        {
            Graph g = new Graph(initial, initial.NodeCount + n);

            for (int i = initial.NodeCount; i < g.NodeCount; i++)
            {
                int j = r.Next(i);
                g[i, j] = true;

                var pv = r.NextDouble();
                int[] neighborsOfJ = g.GetNeighbors(j).ToArray();
                if (pv <= p)
                {
                    int neighborOfJ = neighborsOfJ[r.Next(neighborsOfJ.Length)];
                    g[i, neighborOfJ] = true;
                }
                else // if(pv <= (1 - p))
                {
                    int totalRandomNewNeighbor;
                    do
                    {
                        totalRandomNewNeighbor = r.Next(i);
                    } while (neighborsOfJ.Contains(totalRandomNewNeighbor));
                    g[i, totalRandomNewNeighbor] = true;
                }
            }

            return g;
        }

        async Task ExportGraphToCSVAsync(Graph g, string filename)
        {
            using (var sw = new StreamWriter(filename))
                for (int from = 0; from < g.NodeCount; from++)
                    for (int to = 0; to < g.NodeCount; to++)
                        if(g[from,to])
                            await sw.WriteLineAsync($"{from};{to}");
        }

        async static Task Main(string[] args)
        {
            const int M0 = 5;

            const float P = 0.6f;
            const int N = 1000;

            Program p = new Program();
            Graph initial = p.RandomGraph(M0, 10);
            var g = p.Generate(initial, N, P);
            await p.ExportGraphToCSVAsync(g, $"graph-with-communities-{P}.csv");
        }
    }
}
