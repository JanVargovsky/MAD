using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MAD2.Lesson8
{
    class Program
    {
        async Task<IEnumerable<IrisData>> LoadIrisDataAsync(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            IrisData Parse(string line)
            {
                float ParseFloat(string s) => float.Parse(s);

                var tokens = line.Split(',');
                return new IrisData
                {
                    SepalLength = ParseFloat(tokens[0]),
                    SepalWidth = ParseFloat(tokens[1]),
                    PetalLength = ParseFloat(tokens[2]),
                    PetalWidth = ParseFloat(tokens[3]),
                    Class = tokens[4],
                };
            }
            return lines.Select(Parse);
        }

        double EuclidDistance(IrisData a, IrisData b)
        {
            return Math.Sqrt(
                Math.Pow(a.PetalLength - b.PetalLength, 2) +
                Math.Pow(a.PetalWidth - b.PetalWidth, 2) +
                Math.Pow(a.SepalLength - b.SepalLength, 2) +
                Math.Pow(a.SepalWidth - b.SepalWidth, 2));
        }

        // aka a similarity function
        double CalculateGaussianKernel(IrisData a, IrisData b)
        {
            double distance = EuclidDistance(a, b);
            return Math.Exp(-distance / 2);
        }

        Matrix<double> CalculateDistanceMatrix<T>(IList<T> data, Func<T, T, double> distanceFunc)
        {
            int size = data.Count;
            Matrix<double> distanceMatrix = new Matrix<double>(size);

            // calculate for all
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    distanceMatrix[i, j] = distanceFunc(data[i], data[j]);
            return distanceMatrix;
        }

        Matrix<double> CalculateSimilarityMatrix_EpsilonRadius<T>(IList<T> data, Func<T, T, double> similarityFunc, double e)
        {
            int size = data.Count;
            Matrix<double> distanceMatrix = CalculateDistanceMatrix(data, similarityFunc);

            Matrix<double> similarityMatix = new Matrix<double>(size);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    if (distanceMatrix[i, j] <= e)
                        similarityMatix[i, j] = distanceMatrix[i, j];
            return similarityMatix;
        }

        int Degree(Matrix<double> matrix, int i) =>
            Enumerable.Range(0, matrix.Size)
            .Count(j => matrix[i, j] > 0d);

        // kategoricke atributy
        int KroneckerDelta(IList<IrisData> data, int i, int j) =>
            data[i].Class == data[j].Class ? 1 : 0;

        double CalculateQ(Matrix<double> matrix, List<IrisData> nodes)
        {
            int CalculateEdgeCount()
            {
                int count = 0;
                for (int i = 0; i < matrix.Size; i++)
                    for (int j = 0; j < matrix.Size; j++)
                        if (matrix[i, j] > 0)
                            count++;
                return count;
            }

            // m = number of edges
            var m = CalculateEdgeCount();

            double sum = 0;
            for (int i = 0; i < matrix.Size; i++)
            {
                var ki = Degree(matrix,i);
                for (int j = 0; j < matrix.Size; j++)
                {
                    var A = matrix[i, j];
                    var kj = Degree(matrix, j);
                    var delta = KroneckerDelta(nodes, i, j);
                    var k = ki * kj;
                    sum += ((A - (k / (2d * m))) * delta);
                }
            }

            var Q = 1d / (2d * m) * sum;
            return Q;
        }

        async Task ExportToGDFAsync<T, N>(Matrix<T> matrix, IList<N> nodes, string filename, Func<T, bool> filter, Func<N, string> classSelector)
        {
            using (var sw = new StreamWriter(filename))
            {
                await sw.WriteLineAsync($"nodedef>name VARCHAR, label VARCHAR, class VARCHAR");
                for (int i = 0; i < nodes.Count; i++)
                    await sw.WriteLineAsync($"s{i}, node{i}, {classSelector(nodes[i])}");

                await sw.WriteLineAsync($"edgedef>node1 VARCHAR, node2 VARCHAR, weight DOUBLE");
                for (int i = 0; i < matrix.Size; i++)
                    for (int j = i + 1; j < matrix.Size; j++)
                        if (filter(matrix[i, j]))
                            await sw.WriteLineAsync($"s{i}, s{j}, {matrix[i, j]}");
            }
        }

        Task ExportIrisDataToGDFAsync(Matrix<double> matrix, IList<IrisData> nodes, string filename) =>
            ExportToGDFAsync(matrix, nodes, filename, t => t > 0, t => t.Class);

        static async Task Main(string[] args)
        {
            const string Filename = "../../../../Datasets/iris.data.txt";
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var p = new Program();

            var irisDataSet = (await p.LoadIrisDataAsync(Filename)).ToList();

            const double E = 0.1d;
            var similarityEpsilonMatrix = p.CalculateSimilarityMatrix_EpsilonRadius(irisDataSet, p.CalculateGaussianKernel, E);
            var Q = p.CalculateQ(similarityEpsilonMatrix, irisDataSet);
            Console.WriteLine($"Q={Q}");
            await p.ExportIrisDataToGDFAsync(similarityEpsilonMatrix, irisDataSet, $"EpsilonMatrix_{E}_{Q}.gdf");
        }
    }
}
