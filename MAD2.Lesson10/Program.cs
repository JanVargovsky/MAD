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
                    if (value == 0) value = double.PositiveInfinity;
                    matrix.SetRaw(i, j, value);
                }

            for (int k = 0; k < matrix.Size; k++)
                for (int i = 0; i < matrix.Size; i++)
                    for (int j = 0; j < matrix.Size; j++)
                    {
                        var newLength = matrix.GetRaw(i, k) + matrix.GetRaw(k, j);
                        if (matrix.GetRaw(i, j) > newLength)
                        {
                            //Console.WriteLine($"[{i},{j}] New distance from {matrix[i, j]} to {newLength}");
                            matrix.SetRaw(i, j, (int)newLength);
                        }
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

        static async Task Main(string[] args)
        {
            const string Filename = "../../../../Datasets/ht09_contact_list.dat";
            const int TimeSplit = 60 * 60 * 24;
            var p = new Program();

            var (nodes, matrices) = await p.LoadTemporaryNetworksAsync(Filename, TimeSplit);

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
