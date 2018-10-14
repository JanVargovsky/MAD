using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using static System.ExtendedConsole;

namespace MAD3.Lesson3
{
    class Program
    {
        async Task<List<double[]>> LoadCSVAsync(string filename)
        {
            string path = Path.Combine("../../../../Datasets/", filename);
            var lines = await File.ReadAllLinesAsync(path);
            var result = new List<double[]>();
            foreach (var line in lines)
            {
                var tokens = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var parsed = tokens.Select(double.Parse).ToArray();
                result.Add(parsed);
            }
            return result;
        }

        double EuclideanDistance(double[] a, double[] b)
        {
            var sum = a.Zip(b, (i, j) => Math.Pow(i - j, 2)).Sum();
            return Math.Sqrt(sum);
        }

        DistanceMatrix GetDistanceMatrix<T>(List<T[]> inputMatrix, Func<T[], T[], double> distanceFunction)
        {
            var distanceMatrix = new DistanceMatrix(inputMatrix.Count);

            for (int i = 0; i < distanceMatrix.Size; i++)
                for (int j = 0; j < distanceMatrix.Size; j++)
                {
                    var distance = distanceFunction(inputMatrix[i], inputMatrix[j]);
                    distanceMatrix[i, j] = distance;
                }

            return distanceMatrix;
        }

        async Task ExportAsync(string filename, List<double[]> dataset, List<List<int>> clusters)
        {
            string path = Path.Combine("../../../../MAD3.Lesson3.Visualization/Data", filename);
            using (var sw = new StreamWriter(path))
            {
                for (int i = 0; i < clusters.Count; i++)
                {
                    foreach (var item in clusters[i])
                    {
                        var point = dataset[item];
                        await sw.WriteLineAsync($"{point[0]};{point[1]};{i}");
                    }
                }
            }
        }

        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            (string filename, int clusters) dataset1 = ("clusters3.csv", 3);
            (string filename, int clusters) dataset2 = ("clusters5.csv", 5);
            (string filename, int clusters) dataset3 = ("clusters5n.csv", 5);
            (string filename, int clusters) dataset4 = ("annulus.csv", 2);
            (string filename, int clusters) dataset5 = ("boxes.csv", 5);
            var datasets = new[] { dataset1, dataset2, dataset3, dataset4, dataset5 };

            var p = new Program();
            var clustering = new HierarchicalAgglomerativeClustering();
            foreach (var (filename, clusters) in datasets)
            {
                WriteLine($"Dataset: {filename}");
                WriteLine($"Expected clusters: {clusters}");
                var data = await p.LoadCSVAsync(filename);
                var distanceMatrix = p.GetDistanceMatrix(data, p.EuclideanDistance);

                var sw = Stopwatch.StartNew();
                var singleLinkage = clustering.SingleLinkage(distanceMatrix, t => t.Clusters > clusters);
                sw.Stop();
                WriteLine(ConsoleColor.Yellow, $"Single linkage [{sw.ElapsedMilliseconds}ms]");
                WriteLine(ConsoleColor.Green, string.Join(", ", singleLinkage.Select(t => t.Count)));

                sw = Stopwatch.StartNew();
                var completeLinkage = clustering.CompleteLinkage(distanceMatrix, t => t.Clusters > clusters);
                sw.Stop();
                WriteLine(ConsoleColor.Yellow, $"Complete linkage [{sw.ElapsedMilliseconds}ms]");
                WriteLine(ConsoleColor.Green, string.Join(", ", completeLinkage.Select(t => t.Count)));

                await Task.WhenAll(
                    p.ExportAsync($"SingleLinkage-{filename}", data, singleLinkage),
                    p.ExportAsync($"CompleteLinkage-{filename}", data, completeLinkage));
            }
        }
    }
}
