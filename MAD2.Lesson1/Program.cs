using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MAD2.Lesson1
{
    public class Matrix<T> : IEnumerable<T>
    {
        readonly T[,] data;
        int indexOffset = 0;

        public T this[int row, int col]
        {
            get => data[row + indexOffset, col + indexOffset];
            set => data[row + indexOffset, col + indexOffset] = value;
        }

        public int Size => data.GetLength(0);

        public Matrix(int size)
        {
            data = new T[size, size];
        }

        public Matrix<T> WithIndexOffset(int offset)
        {
            indexOffset = offset;
            return this;
        }

        public IEnumerator<T> GetEnumerator() => data.Cast<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    }

    class Program
    {
        [DebuggerDisplay("{SepalLength}, {SepalWidth}, {PetalLength}, {PetalWidth}")]
        public class IrisData
        {
            public float SepalLength { get; set; }
            public float SepalWidth { get; set; }
            public float PetalLength { get; set; }
            public float PetalWidth { get; set; }
            public string Name { get; set; }
        }

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
                    Name = tokens[4],
                };
            }
            return lines.Select(Parse);
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

        // k nearest neighbors
        Matrix<double> CalculateSimilarityMatrix_KNN<T>(IList<T> data, Func<T, T, double> similarityFunc, int k)
        {
            int size = data.Count;
            Matrix<double> distanceMatrix = CalculateDistanceMatrix(data, similarityFunc);

            IEnumerable<int> GetKNearest(int row)
            {
                var rowValues = new List<(double Value, int Index)>();

                for (int i = 0; i < size; i++)
                {
                    if (row == i) continue;
                    rowValues.Add((distanceMatrix[row, i], i));
                }

                return rowValues.OrderByDescending(t => t.Value).Take(k).Select(t => t.Index);
            }

            Matrix<double> similarityMatix = new Matrix<double>(size);
            for (int row = 0; row < size; row++)
            {
                var kNearest = GetKNearest(row);
                foreach (var i in kNearest)
                    similarityMatix[row, i] = distanceMatrix[row, i];
            }
            return similarityMatix;
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
            ExportToGDFAsync(matrix, nodes, filename, t => t > 0, t => t.Name);

        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            const string Filename = "../Datasets/iris.data.txt";
            var p = new Program();
            var irisDataSet = (await p.LoadIrisDataAsync(Filename)).ToList();

            const double E = 0.4d;
            var similarityEpsilon = p.CalculateSimilarityMatrix_EpsilonRadius(irisDataSet, p.CalculateGaussianKernel, E);
            await p.ExportIrisDataToGDFAsync(similarityEpsilon, irisDataSet, $"export/similarity_epsilon_{E}.gdf");

            const int K = 50;
            var similarityKNN = p.CalculateSimilarityMatrix_KNN(irisDataSet, p.CalculateGaussianKernel, K);
            await p.ExportIrisDataToGDFAsync(similarityKNN, irisDataSet, $"export/similarity_knn_{K}.gdf");
        }
    }
}
