using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        static async Task ExportAsync(string filename, List<double[]> dataset, List<List<int>> clusters)
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
            (string name, int clusters) dataset1 = ("clusters3.csv", 3);
            (string name, int clusters) dataset2 = ("clusters5.csv", 5);
            (string name, int clusters) dataset3 = ("clusters5n.csv", 5);
            (string name, int clusters) dataset4 = ("annulus.csv", 2);
            (string name, int clusters) dataset5 = ("boxes.csv", 5);
            var datasets = new[] { dataset1, dataset2, dataset3, dataset4, dataset5 };

            var p = new Program();
            var clustering = new HierarchicalAgglomerativeClustering();
            foreach (var dataset in datasets)
            {
                Console.WriteLine($"Dataset: {dataset.name}");
                Console.WriteLine($"Expected clusters: {dataset.clusters}");
                var data = await p.LoadCSVAsync(dataset.name);
                var distanceMatrix = p.GetDistanceMatrix(data, p.EuclideanDistance);

                var singleLinkage = clustering.SingleLinkage(distanceMatrix, t => t.Clusters > dataset.clusters);
                Console.WriteLine("Single linkage");
                Console.WriteLine(string.Join(", ", singleLinkage.Select(t => t.Count)));

                var completeLinkage = clustering.CompleteLinkage(distanceMatrix, t => t.Clusters > dataset.clusters);
                Console.WriteLine("Complete linkage");
                Console.WriteLine(string.Join(", ", completeLinkage.Select(t => t.Count)));

                await Task.WhenAll(
                    ExportAsync($"SingleLinkage-{dataset.name}", data, singleLinkage),
                    ExportAsync($"CompleteLinkage-{dataset.name}", data, completeLinkage)
                    );
            }
        }
    }
}
