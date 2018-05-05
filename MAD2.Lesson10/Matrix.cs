using System.Collections.Generic;
using System.Linq;

namespace MAD2.Lesson10
{
    class Matrix<T>
    {
        readonly HashSet<(T From, T To)> edges;

        public Matrix()
        {
            edges = new HashSet<(T From, T To)>();
        }

        public void AddEdge(T a, T b)
        {
            edges.Add((a, b));
            edges.Add((b, a));
        }

        public int NumberMissingOfEdges<T>(T from) =>
            edges.Count(t => !t.From.Equals(from));

        public int NumberOfEdges<T>(T from) =>
            edges.Count(t => t.From.Equals(from));
    }
}
