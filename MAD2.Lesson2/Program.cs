using Accord.MachineLearning;
using Accord.Math.Decompositions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MAD2.Lesson2
{
    public class Matrix<T> : IEnumerable<T>
    {
        internal T[,] Data { get; }
        int indexOffset = 0;

        public T this[int row, int col]
        {
            get => Data[row + indexOffset, col + indexOffset];
            set => Data[row + indexOffset, col + indexOffset] = value;
        }

        public int Size => Data.GetLength(0);

        public Matrix(int size)
        {
            Data = new T[size, size];
        }

        public Matrix<T> WithIndexOffset(int offset)
        {
            indexOffset = offset;
            return this;
        }

        public IEnumerator<T> GetEnumerator() => Data.Cast<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
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

        Matrix<double> CalculateMatrix<T>(IList<T> data, Func<T, T, double> func)
        {
            int size = data.Count;
            Matrix<double> distanceMatrix = new Matrix<double>(size);

            // calculate for all
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    distanceMatrix[i, j] = func(data[i], data[j]);
            return distanceMatrix;
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

        Matrix<double> CalculateDegreeMatrix(Matrix<double> matrix)
        {
            var size = matrix.Size;
            var result = new Matrix<double>(size);

            for (int i = 0; i < size; i++)
                result[i, i] = Enumerable.Range(0, size).Sum(j => matrix[i, j]);

            return result;
        }

        Matrix<double> CalculateLaplacianMatrix(Matrix<double> degreeMatrix, Matrix<double> matrix)
        {
            if (degreeMatrix.Size != matrix.Size) throw new Exception("Invalid size of matrices");

            var result = new Matrix<double>(degreeMatrix.Size);

            for (int i = 0; i < result.Size; i++)
                for (int j = 0; j < result.Size; j++)
                    result[i, j] = degreeMatrix[i, j] - matrix[i, j];

            return result;
        }

        void SpectralClustering(IList<IrisData> data, int k)
        {
            // 1. similarity matrix
            var A = CalculateMatrix(data, CalculateGaussianKernel);

            // 2. if ratio cut then B <- L
            // whats ratio cut?
            var B = CalculateLaplacianMatrix(A, CalculateDegreeMatrix(A));
            // 3. B <- L^s or L^a

            // 4. Solve B*u = Lambda*u
            var eigen = new EigenvalueDecomposition(B.Data);
            //var eigenValues = 0;
            //var eigenVector = 0;

            // 5. U <- (u_n, u_n-1, ..., u_n-k+1)
            var U = new Matrix<double>(B.Size);
            for (int row = 0; row < U.Size; row++)
                for (int n = U.Size - 1; n >= 0; n--)
                    U[row, n] = eigen.Eigenvectors[n, n];

            var Y = new Matrix<double>(U.Size);
            double CalculateY(int i)
            {
                return 42;
            }

            var kmeans = new KMeans(k);
        }

        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            const string Filename = "../Datasets/iris.data.txt";
            var p = new Program();
            var irisDataSet = (await p.LoadIrisDataAsync(Filename)).ToList();

            p.SpectralClustering(irisDataSet, 3);

            var similarityMatrix = p.CalculateMatrix(irisDataSet, p.CalculateGaussianKernel);
            var degreeMatrix = p.CalculateDegreeMatrix(similarityMatrix);
            var laplacianMatrix = p.CalculateLaplacianMatrix(degreeMatrix, similarityMatrix);

            // dodelat spektralni rozklad - bud vlastni skrz vlastni cisla
            // nebo primo knihovna co mi vypocte primo Spectral Clustering Algorithm

        }
    }
}
