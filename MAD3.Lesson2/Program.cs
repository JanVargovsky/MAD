using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MAD3.Lesson2
{
    class Program
    {
        static async Task<RowDataset> LoadDatasetAsync(string filename)
        {
            string path = Path.Combine("../../../../Datasets/", filename);
            var lines = await File.ReadAllLinesAsync(path);
            var result = new RowDataset();
            foreach (var line in lines)
            {
                if (line == string.Empty) continue;

                var tokens = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var parsed = tokens.Select(double.Parse).ToArray();
                result.Rows.Add(parsed);
            }
            return result;
        }

        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dataset1 = "test.csv";
            string dataset2 = "wholesale.csv";
            string dataset3 = "wine.csv";
            string dataset4 = "water.csv";
            var datasets = new[] { dataset1, dataset2, dataset3, dataset4 };

            var clusteringFeatureSelection = new ClusteringFeatureSelection();
            //var datasetName = dataset1;

            const int m = 10;

            foreach (var datasetName in datasets)
            {
                var dataset = await LoadDatasetAsync(datasetName);
                var columnDataset = dataset.AsColumnDataset();

                Console.WriteLine($"Filename: {datasetName} ({dataset.NumberOfAttributes})");
                // Original values
                //var entropy = clusteringFeatureSelection.Entropy(dataset, m);
                //Console.WriteLine($"m={m}, entropy={entropy}");

                // Optimized
                //var entropy = clusteringFeatureSelection.Entropy(dataset, m);
                var minEntropyResult = clusteringFeatureSelection.GetMinimalEntropy(columnDataset, m);

                Console.WriteLine($"Original attributes={minEntropyResult.OriginalDataset.NumberOfAttributes}, entropy={minEntropyResult.OriginalDatasetEntropy}");
                Console.WriteLine($"Minimal attributes={minEntropyResult.MinimizedDataset.NumberOfAttributes}, entropy={minEntropyResult.MinimizedDatasetEntropy}");
            }
        }
    }
}
