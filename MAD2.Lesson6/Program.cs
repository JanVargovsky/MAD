using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        Graph Circle(int nodes)
        {
            var g = Graph.Empty;

            void AddEdge(int from, int to) => g.Edges.Add(new Edge(from, to));
            void AddNode(int i) => g.Nodes.Add(i);

            for (int i = 0; i < nodes - 1; i++)
            {
                AddNode(i);
                AddEdge(i, i + 1);
            }
            AddNode(nodes - 1);
            AddEdge(0, nodes - 1);

            return g;
        }

        Task ExportGraphAsync(string filename, Graph g) => File.WriteAllLinesAsync(filename, g.Edges.Select(e => $"{e.From};{e.To}"));

        static async Task Main(string[] args)
        {
            var p = new Program();
            {
                const int M = 50;
                const double P = 0.5d;
                var evolvingNetworks = new EvolvingNetworks();

                var initial = p.CompleteGraph(5);
                var linkSelectionModel = evolvingNetworks.LinkSelectionModel(initial, M);
                var copyingModel = evolvingNetworks.CopyingModel(initial, M, P);
            }

            {
                const int M = 2; // number of links for each new node
                const int N = 2; // number of new internal links after each new node
                const int T = 500; // desired node count
                var barabasiAlbert = new BarabasiAlbert();

                var initial = p.Circle(10);
                var basicBA = barabasiAlbert.Basic(initial, M, T);
                var internalLinksDoublePreferentialAttachmentBA = barabasiAlbert.InternalLinks(initial, M, T, N, BarabasiAlbert.InternalLinksStrategy.DoublePreferentialAttachment);
                var internalLinksRandomAttachmentBA = barabasiAlbert.InternalLinks(initial, M, T, N, BarabasiAlbert.InternalLinksStrategy.RandomAttachment);

                await Task.WhenAll(
                    p.ExportGraphAsync($"ba-{M}-{T}.csv", basicBA),
                    p.ExportGraphAsync($"ba-internal-links-double-preferential-attachment-{M}-{T}.csv", internalLinksDoublePreferentialAttachmentBA),
                    p.ExportGraphAsync($"ba-internal-links-random-attachment-{M}-{T}.csv", internalLinksRandomAttachmentBA)
                    );
            }
        }
    }
}
