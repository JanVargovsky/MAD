using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MAD2.Lesson7
{
    public class Graph
    {
        public List<Edge> Edges { get; set; }
        public List<int> Nodes { get; set; }

        public static Graph Empty => new Graph
        {
            Edges = new List<Edge>(),
            Nodes = new List<int>(),
        };

        public Graph Copy() => new Graph
        {
            Edges = new List<Edge>(Edges),
            Nodes = new List<int>(Nodes)
        };

        public int Degree(int i) => Edges.Count(t => t.From == i || t.To == i);

        public void Remove(int n)
        {
            Nodes.Remove(n);
            Edges.RemoveAll(e => e.From == n || e.To == n);
        }
    }

    [DebuggerDisplay("({From}, {To})")]
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
}
