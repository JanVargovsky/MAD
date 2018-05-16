using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MAD2.Project
{
    public class DatasetLoader
    {
        public async Task<List<Edge>> LoadDatasetAsync(string path)
        {
            using (var sr = new StreamReader(path))
            {
                string line;
                string[] tokens;

                line = await sr.ReadLineAsync();
                if (line != "dl")
                    throw new Exception("Invalid file format");

                line = await sr.ReadLineAsync();
                tokens = line.Split('=');
                if (tokens.Length < 2 || tokens[0] != "N" || !int.TryParse(tokens[1], out var N))
                    throw new Exception("Invalid node count");

                line = await sr.ReadLineAsync();
                tokens = line.Split('=');
                if (tokens.Length < 2 || tokens[0] != "format" || tokens[1] != "edgelist1")
                    throw new Exception("Unknown format");

                line = await sr.ReadLineAsync();
                if (!line.StartsWith("data", StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("No data label");

                var result = new List<Edge>();
                int[] edge = new int[3];
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    tokens = line.Split(' ');

                    if (tokens.Length != edge.Length) throw new Exception("Invalid length of edge");

                    for (int i = 0; i < edge.Length; i++)
                        if (!int.TryParse(tokens[i], out edge[i]))
                            throw new Exception("Invalid edge data");

                    result.Add(new Edge(edge[0], edge[1], edge[2]));
                }
                return result;
            }
        }

        public List<Edge> NormalizeIndexes(List<Edge> edges, int start = 0)
        {
            int id = start;
            var indexes = new Dictionary<int, int>(); // <old, new>
            var result = new List<Edge>();

            foreach (var edge in edges)
            {
                if (!indexes.TryGetValue(edge.NodeFrom, out var nodeFrom))
                    nodeFrom = indexes[edge.NodeFrom] = id++;
                if (!indexes.TryGetValue(edge.NodeTo, out var nodeTo))
                    nodeTo = indexes[edge.NodeTo] = id++;

                result.Add(new Edge(nodeFrom, nodeTo, edge.Weight));
            }

            return result;
        }

        public IEnumerable<int> GetNodes(IEnumerable<Edge> edges)
        {
            var nodes = new HashSet<int>();
            foreach (var edge in edges)
            {
                nodes.Add(edge.NodeFrom);
                nodes.Add(edge.NodeTo);
            }
            return nodes;
        }

        public Matrix<Edge> GetAdjacencyMatrixWithEdges(IEnumerable<Edge> edges, IEnumerable<Node> nodes)
        {
            int min = nodes.Min(t => t.Id);
            int max = nodes.Max(t => t.Id);
            int size = max - min + 1;
            var matrix = new Matrix<Edge>(size)
                .WithIndexOffset(-min);

            foreach (var edge in edges)
                matrix[edge.NodeFrom, edge.NodeTo] = edge;

            return matrix;
        }

        public Matrix<int> GetAdjacencyMatrix(IEnumerable<Edge> edges, IEnumerable<Node> nodes)
        {
            int min = nodes.Min(t => t.Id);
            int max = nodes.Max(t => t.Id);
            int size = max - min + 1;
            var matrix = new Matrix<int>(size)
                .WithIndexOffset(-min);

            foreach (var edge in edges)
                matrix[edge.NodeFrom, edge.NodeTo] = edge.Weight;

            return matrix;
        }

        public async Task<List<Node>> LoadNodesAsync(string path)
        {
            const string Pattern = " (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";

            using (var sr = new StreamReader(path))
            {
                string line;
                string[] tokens;

                var headers = Regex.Split(await sr.ReadLineAsync(), Pattern);

                var result = new List<Node>();
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    tokens = Regex.Split(line, Pattern);

                    if (headers.Length != tokens.Length)
                        throw new Exception("Invalid node row, header doesnt match");

                    if (!int.TryParse(tokens[0], out var id))
                        throw new Exception("Invalid node id");

                    var attributes = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Length; i++)
                        attributes.Add(headers[i], tokens[i]);

                    result.Add(new Node(id, attributes));
                }
                return result;
            }
        }
    }
}
