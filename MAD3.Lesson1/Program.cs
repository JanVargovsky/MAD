using Accord.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD3.Lesson1
{
    class Program
    {
        void Task1()
        {
            const int N = 6;
            const int K = 3;

            Console.WriteLine("Hardcoded");
            for (int a = 0; a < N; a++)
                for (int b = a + 1; b < N; b++)
                    for (int c = b + 1; c < N; c++)
                        Console.WriteLine($"{a} {b} {c}");

            Console.WriteLine("Generated");
            var data = Enumerable.Range(0, N).ToArray();
            var combinations = Combinatorics.Combinations(data, K);
            foreach (var item in combinations)
                Console.WriteLine(string.Join(' ', item));
        }

        async Task<IList<int[]>> LoadDatasetAsync(string filename)
        {
            string path = Path.Combine("../../../../Datasets/", filename);
            var lines = await File.ReadAllLinesAsync(path);

            var result = new List<int[]>();
            foreach (var line in lines)
            {
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var parsed = tokens.Select(int.Parse).ToArray();
                result.Add(parsed);
            }
            return result;
        }

        Dictionary<int, Dictionary<HashSet<int>, HashSet<int>>> AssociationPatternMining_VerticalCountingMethods(IList<int[]> data, float minSupport)
        {
            float CalculateSupport(HashSet<int> rows) => rows.Count / (float)data.Count;
            bool IsMinSupportSatisfied(HashSet<int> rows) => CalculateSupport(rows) >= minSupport;

            void WriteSupport(IEnumerable<KeyValuePair<HashSet<int>, HashSet<int>>> sets, int k)
            {
                Console.WriteLine($"Rules k={k}");
                foreach (var item in sets)
                {
                    var rule = string.Join(", ", item.Key);
                    var rows = string.Join(", ", item.Value);
                    Console.WriteLine($"Rule = {{{rule}}}, Rows = {{{rows}}}, Support = {item.Value.Count}/{data.Count} ({CalculateSupport(item.Value) * 100}%) ");
                }
            }

            // <Pattern, Rows>
            var F1 = new Dictionary<int, HashSet<int>>();
            // Initial phase
            for (int row = 0; row < data.Count; row++)
            {
                foreach (var attribute in data[row])
                {
                    if (!F1.TryGetValue(attribute, out var rows))
                        F1[attribute] = rows = new HashSet<int>();

                    rows.Add(row + 1);
                }
            }

            // <Pattern, Rows>
            var F = new Dictionary<int, Dictionary<HashSet<int>, HashSet<int>>>();
            F[1] = new Dictionary<HashSet<int>, HashSet<int>>();
            foreach (var item in F1)
            {
                var pattern = new HashSet<int>
                {
                    item.Key
                };
                F[1][pattern] = item.Value;
            }
            WriteSupport(F[1], 1);

            for (int k = 2; k < F1.Count; k++)
            {

                if (F[k - 1].Count == 0)
                    break;
                var newF = new Dictionary<HashSet<int>, HashSet<int>>();
                foreach (var ck in Combinatorics.Combinations(F[k - 1].ToArray(), 2))
                {
                    var newPattern = new HashSet<int>(ck[0].Key);
                    newPattern.UnionWith(ck[1].Key);
                    if (newPattern.Count != k)
                        continue;

                    var newRows = new HashSet<int>(ck[0].Value);
                    newRows.IntersectWith(ck[1].Value);
                    if (!IsMinSupportSatisfied(newRows))
                        continue;
                    newF[newPattern] = newRows;
                }
                if (!newF.Any())
                    break;
                F[k] = newF;
                WriteSupport(F[k], k);
            }
            return F;
        }

        async Task Task2_3Async()
        {
            (string filename, float minSupport, float minConfidence) dataset1 = ("itemsets_test.dat", 0.2f, 0.5f);
            (string filename, float minSupport, float minConfidence) dataset2 = ("chess.dat", 0.95f, 0.5f);
            (string filename, float minSupport, float minConfidence) dataset3 = ("mushroom.dat", 0.7f, 0.5f);
            (string filename, float minSupport, float minConfidence) dataset4 = ("connect.dat", 0.98f, 0.7f);
            (string filename, float minSupport, float minConfidence) dataset5 = ("T10I4D100K.dat", 0.006f, 0.9f);

            var associationPatternMining = new AssociationPatternsMining(false);
            var exporter = new AssociationPatternsMiningExporter();

            var dataset = dataset1;
            var data = await LoadDatasetAsync(dataset.filename);
            var result = associationPatternMining.VerticalCountingMethods(data, dataset.minSupport, dataset.minConfidence);
            exporter.ExportToConsole(dataset.filename, result);
        }

        static async Task Main(string[] args)
        {
            var p = new Program();
            //p.Task1();

            Stopwatch sw = Stopwatch.StartNew();
            await p.Task2_3Async();
            sw.Stop();
            Console.WriteLine($"Done in {sw.Elapsed}");
        }
    }
}
