using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                float ParseFloat(string s) => float.Parse(s.Replace('.', ','));

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

        enum SimilarityAlgorithm
        {
            EpsilonRadius,
            KNN
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

                return rowValues.OrderBy(t => t.Value).Take(k).Select(t => t.Index);
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

        double CalculateGaussianKernel(IrisData a, IrisData b)
        {
            double vectorDistance =
                a.PetalLength - b.PetalLength +
                a.PetalWidth - b.PetalWidth +
                a.SepalLength - b.SepalLength +
                a.SepalWidth - b.SepalWidth;

            return Math.Exp(-Math.Pow(vectorDistance, 2) / 2);
        }

        async Task ExportToGDFAsync<T>(Matrix<T> matrix, string filename, Func<T, bool> filter)
        {
            using (var sw = new StreamWriter(filename))
            {
                await sw.WriteLineAsync($"nodedef>name VARCHAR, label VARCHAR");
                for (int i = 0; i < matrix.Size; i++)
                    await sw.WriteLineAsync($"s{i}, node{i}");

                await sw.WriteLineAsync($"edgedef>node1 VARCHAR, node2 VARCHAR, weight DOUBLE");

                for (int i = 0; i < matrix.Size; i++)
                    for (int j = i + 1; j < matrix.Size; j++)
                        if (filter(matrix[i, j]))
                            await sw.WriteLineAsync($"s{i}, s{j}, {matrix[i, j]}");
            }
        }

        static async Task Main(string[] args)
        {
            const string Filename = "../Datasets/iris.data.txt";
            var p = new Program();
            var irisDataSet = (await p.LoadIrisDataAsync(Filename)).ToList();

            const double E = 0.3d;
            var similarityEpsilon = p.CalculateSimilarityMatrix_EpsilonRadius(irisDataSet, p.CalculateGaussianKernel, E);
            await p.ExportToGDFAsync(similarityEpsilon, $"export/similarity_epsilon_{E}.gdf", t => t > 0);

            const int K = 5;
            var similarityKNN = p.CalculateSimilarityMatrix_KNN(irisDataSet, p.CalculateGaussianKernel, K);
            await p.ExportToGDFAsync(similarityKNN, $"export/similarity_knn_{K}.gdf", t => t > 0);
        }
    }
}
