using System.Collections.Generic;
using System.Diagnostics;

namespace MAD2.Lesson4
{
    public class Graph
    {
        public List<Edge> Edges { get; set; }
        public List<int> Nodes { get; set; }

        public static Graph Empty => new Graph()
        {
            Edges = new List<Edge>(),
            Nodes = new List<int>(),
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
    }
}
