using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MAD3.Lesson2
{
    class Program
    {
        static async Task<Dataset> LoadDatasetAsync(string filename)
        {
            string path = Path.Combine("../../../../Datasets/", filename);
            var lines = await File.ReadAllLinesAsync(path);
            var result = new Dataset();
            foreach (var line in lines)
            {
                if (line == string.Empty) continue;

                var tokens = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var parsed = tokens.Select(double.Parse).ToArray();
                result.Data.Add(parsed);
            }
            return result;
        }

        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dataset1 = "test.csv";
            string dataset2 = "wine.csv";
            string dataset3 = "water.csv";
            string dataset4 = "wholesale.csv";
            var datasets = new[] { dataset1, dataset2, dataset3, dataset4 };

            var clusteringFeatureSelection = new ClusteringFeatureSelection();
            //var datasetName = dataset1;

            foreach (var datasetName in datasets)
            {
                var dataset = await LoadDatasetAsync(datasetName);

                Console.WriteLine($"dataset: {datasetName}");
                for (int m = 2; m <= 10; m++)
                {
                    var entropy = clusteringFeatureSelection.Entropy(dataset, m);
                    Console.WriteLine($"m={m}, entropy={entropy}");
                }
            }
        }
    }
}
