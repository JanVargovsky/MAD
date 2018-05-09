using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD2.Lesson10
{
    class Program
    {
        async Task<(int[] Nodes, SparseMatrix<int>[] Matrices)> LoadTemporaryNetworksToSparseMatrixAsync(string filename, int timeSplit)
        {
            (int TimeStamp, int From, int To) ParseLine(string line)
            {
                var values = line.Split('\t')
                    .Select(int.Parse)
                    .ToArray();
                return (values[0], values[1], values[2]);
            }

            SparseMatrix<int> GroupToMatrix(IEnumerable<(int TimeStamp, int From, int To)> values)
            {
                SparseMatrix<int> matrix = new SparseMatrix<int>();
                values.ForEach(t => matrix.AddEdge(t.From, t.To));
                return matrix;
            }

            var data = (await File.ReadAllLinesAsync(filename))
                .Select(ParseLine)
                .ToArray();

            var nodes = data.Select(t => t.From).Concat(data.Select(t => t.To)).Distinct().ToArray();

            var matrices = data.GroupBy(t => t.TimeStamp / timeSplit)
                .Select(GroupToMatrix)
                .ToArray();
            return (nodes, matrices);
        }

        async Task<(int[] Nodes, Matrix<int>[] Matrices)> LoadTemporaryNetworksAsync(string filename, int timeSplit)
        {
            (int TimeStamp, int From, int To) ParseLine(string line)
            {
                var values = line.Split('\t')
                    .Select(int.Parse)
                    .ToArray();
                return (values[0], values[1], values[2]);
            }

            var data = (await File.ReadAllLinesAsync(filename))
                .Select(ParseLine)
                .ToArray();

            var nodes = data.Select(t => t.From).Concat(data.Select(t => t.To)).Distinct().ToArray();

            var min = nodes.Min();
            var max = nodes.Max();
            var size = max - min + 1;

            Matrix<int> GroupToMatrix(IEnumerable<(int TimeStamp, int From, int To)> values)
            {
                Matrix<int> matrix = new Matrix<int>(size).WithIndexOffset(-min);
                values.ForEach(t =>
                {
                    matrix[t.From, t.To] = matrix[t.To, t.From] = 1;
                });
                return matrix;
            }

            var matrices = data.GroupBy(t => t.TimeStamp / timeSplit)
                .Select(GroupToMatrix)
                .ToArray();
            return (nodes, matrices);
        }

        Matrix<double> FloydWarshall(Matrix<int> inputMatrix)
        {
            var matrix = inputMatrix.EmptyCopy<double>();

            for (int i = 0; i < matrix.Size; i++)
                for (int j = 0; j < matrix.Size; j++)
                {
                    double value = inputMatrix.GetRaw(i, j);
                    if (value == 0 && i != j) value = double.PositiveInfinity;
                    matrix.SetRaw(i, j, value);
                }

            for (int k = 0; k < matrix.Size; k++)
                for (int i = 0; i < matrix.Size; i++)
                    for (int j = 0; j < matrix.Size; j++)
                    {
                        var newLength = matrix.GetRaw(i, k) + matrix.GetRaw(k, j);
                        if (matrix.GetRaw(i, j) > newLength)
                            matrix.SetRaw(i, j, newLength);
                    }

            return matrix;
        }

        IDictionary<int, double> ClosenessCentrality(int[] nodes, Matrix<double> distanceMatrix)
        {
            double N = nodes.Length;
            var result = new Dictionary<int, double>();

            double SumOfWeights(int from) =>
                Enumerable.Range(-distanceMatrix.IndexOffset, distanceMatrix.Size)
                .Where(t => !double.IsPositiveInfinity(distanceMatrix[from, t]))
                .Sum(i => distanceMatrix[from, i]);

            foreach (var node in nodes)
            {
                var sumOfWeights = SumOfWeights(node);
                var value = sumOfWeights == 0 ? N : N / sumOfWeights;
                result[node] = value;
            }
            return result;
        }

        IDictionary<int, int> DegreeCentrality(int[] actors, Matrix<int>[] matrices)
        {
            // sum of degrees among all layers
            int Degree(Matrix<int> m, int from) => 
                Enumerable.Range(-m.IndexOffset, m.Size)
                .Count(i => m[from, i] > 0);

            var result = new Dictionary<int, int>();
            foreach (var actor in actors)
                result[actor] = matrices.Sum(m => Degree(m, actor));
            return result;
        }

        IDictionary<int, int> NeighborhoodCentrality(int[] actors, Matrix<int>[] matrices)
        {
            // sum of unique edges among all layers
            IEnumerable<int> Neighbors(Matrix<int> m, int from) =>
                Enumerable.Range(-m.IndexOffset, m.Size)
                .Where(i => m[from, i] > 0);

            var result = new Dictionary<int, int>();
            foreach (var actor in actors)
                result[actor] = matrices.SelectMany(m => Neighbors(m, actor)).Distinct().Count();
            return result;
        }

        IDictionary<int, double> ConnectiveRedundancy(int[] actors, IDictionary<int, int> neighborhoods, IDictionary<int, int> degrees)
        {
            var result = new Dictionary<int, double>();
            foreach (var actor in actors)
                result[actor] = 1 - neighborhoods[actor] / (double)degrees[actor];
            return result;
        }

        static async Task Main(string[] args)
        {
            const string Filename = "../../../../Datasets/ht09_contact_list.dat";
            const int TimeSplit = 60 * 60 * 24;
            var p = new Program();

            var (nodes, matrices) = await p.LoadTemporaryNetworksAsync(Filename, TimeSplit);

            var degreeCentrality = p.DegreeCentrality(nodes, matrices);
            Console.WriteLine(string.Join(Environment.NewLine,
                degreeCentrality.OrderBy(t => t.Value)
                .Select(t => $"Node={t.Key}, Degree Centrality={t.Value}")));

            var neighborhoodCentrality = p.NeighborhoodCentrality(nodes, matrices);
            Console.WriteLine(string.Join(Environment.NewLine,
                neighborhoodCentrality.OrderBy(t => t.Value)
                .Select(t => $"Node={t.Key}, Neighborhood Centrality={t.Value}")));

            var connectiveRedundancy = p.ConnectiveRedundancy(nodes, neighborhoodCentrality, degreeCentrality);
            Console.WriteLine(string.Join(Environment.NewLine,
                connectiveRedundancy.OrderBy(t => t.Value)
                .Select(t => $"Node={t.Key}, Connective Redundancy={t.Value}")));

            Console.WriteLine(string.Join(Environment.NewLine,
                nodes.OrderBy(t => t)
                .Select(t => $"Node={t}" +
                $", Degree Centrality = {degreeCentrality[t]}".PadRight(25) +
                $", Neighborhood Centrality = {neighborhoodCentrality[t]}".PadRight(30) +
                $", Connective Redundancy = {connectiveRedundancy[t]:F2}".PadRight(20))));

            for (int i = 0; i < matrices.Length; i++)
            {
                Console.WriteLine($"Timestamp = {i * TimeSplit} - {(i + 1) * TimeSplit}");
                var floyd = p.FloydWarshall(matrices[i]);
                var closeness = p.ClosenessCentrality(nodes, floyd);

                Console.WriteLine(string.Join(Environment.NewLine,
                    closeness.OrderBy(t => t.Value)
                    .Select(t => $"Node={t.Key}, Closeness={t.Value}")));
            }
        }
    }
}
