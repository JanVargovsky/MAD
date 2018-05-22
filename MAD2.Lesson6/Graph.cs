using System.Collections.Generic;
using System.Diagnostics;

namespace MAD2.Lesson6
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

        public override bool Equals(object obj)
        {
            return obj is Edge edge &&
                   From == edge.From &&
                   To == edge.To;
        }
    }
}
