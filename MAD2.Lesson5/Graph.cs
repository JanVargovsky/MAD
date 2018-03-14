using System.Collections.Generic;

namespace MAD2.Lesson5
{
    public class Graph
    {
        bool[,] data;

        public bool this[int from, int to]
        {
            get => data[from, to];
            set => data[from, to] = data[to, from] = value;
        }

        public int NodeCount => data.GetLength(0);

        public Graph(int nodeCount)
        {
            data = new bool[nodeCount, nodeCount];
        }

        public Graph(Graph g, int nodeCount)
            : this(nodeCount)
        {
            for (int from = 0; from < g.NodeCount; from++)
                for (int to = 0; to < g.NodeCount; to++)
                    data[from, to] = g.data[from, to];
        }

        public IEnumerable<int> GetNeighbors(int node)
        {
            for (int i = 0; i < NodeCount; i++)
                if (this[node, i])
                    yield return i;
        }
    }
}
