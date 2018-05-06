using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAD2.Project
{
    public class MainViewModel : ViewModelBase
    {
        readonly DatasetLoader datasetLoader;
        readonly NetworkDatasetAnalyser networkDatasetAnalyser;
        readonly List<Edge> edges = new List<Edge>();
        readonly Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        Matrix<Edge> adjacencyMatrix = new Matrix<Edge>(0);

        public int EdgeCount => edges.Count;
        public int NodeCount => nodes.Count;

        private double averageDegree;
        public double AverageDegree
        {
            get { return averageDegree; }
            set
            {
                averageDegree = value;
                NotifyPropertyChanged();
            }
        }

        private double averageWeightedDegree;
        public double AverageWeightedDegree
        {
            get { return averageWeightedDegree; }
            set
            {
                averageWeightedDegree = value;
                NotifyPropertyChanged();
            }
        }

        private List<int>[] weaklyConnectedComponents = new List<int>[0];
        public List<int>[] WeaklyConnectedComponents
        {
            get { return weaklyConnectedComponents; }
            set
            {
                weaklyConnectedComponents = value;
                NotifyPropertyChanged();
            }
        }

        private double modularity = 0d;
        public double Modularity
        {
            get { return modularity; }
            set
            {
                modularity = value;
                NotifyPropertyChanged();
            }
        }

        public MainViewModel()
        {
            datasetLoader = new DatasetLoader();
            networkDatasetAnalyser = new NetworkDatasetAnalyser();
        }

        public async Task LoadDatasetAsync(string path)
        {
            edges.Clear();
            var loadedEdges = await datasetLoader.LoadDatasetAsync(path);
            edges.AddRange(loadedEdges);
            NotifyPropertyChanged(nameof(EdgeCount));

            var loadedNodes = datasetLoader.GetNodes(edges);
            nodes.Clear();
            foreach (var node in loadedNodes)
                nodes[node] = new Node(node, new Dictionary<string, string>());
            NotifyPropertyChanged(nameof(NodeCount));

            adjacencyMatrix = datasetLoader.GetAdjacencyMatrix(edges, nodes.Values);
        }

        public async Task LoadNodeInformationAsync(string path)
        {
            var loadedNodes = await datasetLoader.LoadNodesAsync(path);
            foreach (var n in loadedNodes)
            {
                if (!nodes.TryGetValue(n.Id, out var node))
                    continue;

                foreach (var nodeAttr in node.Attributes)
                    node.Attributes[nodeAttr.Key] = nodeAttr.Value;
            }

            NotifyPropertyChanged(nameof(NodeCount));
        }

        public void BasicAnalysis()
        {
            Task.Run(() =>
            {
                AverageDegree = networkDatasetAnalyser.AverageDegree(edges, nodes);
                AverageWeightedDegree = networkDatasetAnalyser.AverageWeightedDegree(edges, nodes);
                WeaklyConnectedComponents = networkDatasetAnalyser.WeaklyConnectedComponents(edges, nodes.Keys);
            });
        }

        public void CalculateModularity()
        {
            Task.Run(() => Modularity = networkDatasetAnalyser.Modularity(adjacencyMatrix, edges));
        }
    }
}
