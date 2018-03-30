using System;
using System.Linq;

namespace MAD2.Lesson6
{
    class EvolvingNetworks
    {
        readonly Random random;

        public EvolvingNetworks()
        {
            random = new Random(42);
        }

        public Graph LinkSelectionModel(Graph initial, int m)
        {
            Graph g = initial.Copy();
            var id = g.Nodes.Max() + 1;
            while (m-- > 0)
            {
                var edge = g.Edges[random.Next(g.Edges.Count)];
                var node = random.NextBool() ? edge.From : edge.To;

                g.Edges.Add(new Edge(node, id));
                g.Nodes.Add(id++);
            }

            return g;
        }

        public Graph CopyingModel(Graph initial, int m, double p)
        {
            Graph g = initial.Copy();
            var id = g.Nodes.Max() + 1;
            while (m-- > 0)
            {
                var u = g.Nodes[random.Next(g.Nodes.Count)];

                double r = random.NextDouble();
                if (r <= p)
                {
                    // random connection
                    g.Edges.Add(new Edge(id, u));
                }
                else
                {
                    // copying
                    var edges = g.Edges.Where(t => t.From == u).ToArray();
                    var edgeOfU = edges[random.Next(edges.Length)];
                    var target = edgeOfU.To;

                    g.Edges.Add(new Edge(id, target));
                }
                g.Nodes.Add(id++);
            }

            return g;
        }
    }
}
