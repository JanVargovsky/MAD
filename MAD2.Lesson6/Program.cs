namespace MAD2.Lesson6
{
    class Program
    {
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

        static void Main(string[] args)
        {
            const int M = 50;
            const double P = 0.5d;

            var p = new Program();
            var initial = p.CompleteGraph(5);
            var evolvingNetworks = new EvolvingNetworks();

            var linkSelectionModel = evolvingNetworks.LinkSelectionModel(initial, M);
            var copyingModel = evolvingNetworks.CopyingModel(initial, M, P);
        }
    }
}
