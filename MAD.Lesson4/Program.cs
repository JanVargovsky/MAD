using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson4
{
    class Program
    {
        async Task<int[,]> LoadMatrixAsync(string file, int size)
        {
            var matrix = new int[size, size];

            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines)
            {
                var tokens = line.Split(';');
                var from = int.Parse(tokens[0]);
                var to = int.Parse(tokens[1]);

                matrix[from, to] = matrix[to, from] = 1;
            }

            return matrix;
        }

        double[] ClusterAnalysis(int[,] matrix)
        {
            var size = matrix.GetLength(0);
            var result = new double[size];
            int n = size - 1;
            for (int i = 1; i < size; i++)
            {
                int neighbors = 0;
                for (int j = 1; j < size; j++)
                    if (matrix[i, j] > 0)
                        neighbors++;
                int maximumNumberOfEdges = neighbors * (neighbors - 1);

                int numberOfEdges = 0;
                for (int j = 1; j < size; j++)
                {
                    for (int k = j + 1; k < size; k++)
                    {
                        // j and k are neighbors of i
                        if (matrix[i, j] > 0 && matrix[i, k] > 0)
                        {
                            // j and k are neighbors
                            if (matrix[j, k] > 0)
                                numberOfEdges++;
                        }
                    }
                }
                if (numberOfEdges != 0 || maximumNumberOfEdges != 0)
                    result[i] = 2d * numberOfEdges / maximumNumberOfEdges;
            }
            return result;
        }

        int[,] GenerateGraph(int n, float p)
        {
            var result = new int[n, n];
            var r = new Random();
            int m = (int)Math.Ceiling(p * n * (n - 1) / 2);

            while (m > 0)
            {
                var i = r.Next(1, n);
                var j = r.Next(1, n);

                if (i != j && result[i, j] == 0)
                {
                    result[i, j] = result[j, i] = 1;
                    m--;
                }
            }

            return result;
        }

        void ExportGraph(int[,] matrix, string filename)
        {
            using (var sw = new StreamWriter(filename))
            {
                var size = matrix.GetLength(0);
                for (int i = 1; i < size; i++)
                    for (int j = i + 1; j < size; j++)
                        if (matrix[i, j] > 0)
                            sw.WriteLine($"{i},{j}");
            }
        }

        public void WriteAll(int[,] matrix)
        {
            var clusterAnalysis = ClusterAnalysis(matrix);
            Console.WriteLine($"Prumerny shlukovaci koeficient: {clusterAnalysis.Skip(1).Average():n3}");
            Console.WriteLine("Shlukovací koeficienty");
            for (int i = 1; i < clusterAnalysis.Length; i++)
                Console.WriteLine($"{i} = {clusterAnalysis[i]:n3}");
        }

        static async Task Main(string[] args)
        {
            const string Filename = "KarateClub.csv";
            var p = new Program();
            var matrix = await p.LoadMatrixAsync(Filename, 34 + 1);

            var clusterAnalysis = p.ClusterAnalysis(matrix);
            Console.WriteLine($"Prumerny shlukovaci koeficient: {clusterAnalysis.Skip(1).Average():n3}");
            Console.WriteLine("Shlukovací koeficienty");
            for (int i = 1; i < clusterAnalysis.Length; i++)
                Console.WriteLine($"{i} = {clusterAnalysis[i]:n3}");

            foreach (var prob in new[] { 0.3f, 0.5f, 0.7f })
            {
                Console.Write(new string('=', Console.WindowWidth));
                Console.WriteLine($"Nahodny graf (p={prob}):");
                Console.Write(new string('=', Console.WindowWidth));
                var randomGraph = p.GenerateGraph(35, prob);
                //p.ExportGraph(randomGraph, $"{nameof(randomGraph)}.csv");

                new Lesson2.Program().WriteAll(randomGraph);
                new Lesson3.Program().WriteAll(randomGraph);
                p.WriteAll(randomGraph);
            }
        }
    }
}
