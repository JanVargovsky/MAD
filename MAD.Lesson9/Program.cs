using MAD.Lesson6;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson9
{
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

        // cetnost = pocet_vyskytu / pocet_zaznamu [empiricka]
        // normalni distribuce [teoreticka hodnota]

        float NormalDistribution(float x, float u, float o2)
        {
            var a = 1f / Math.Sqrt(2 * Math.PI * o2);
            var b = Math.Pow(Math.E, -Math.Pow(x - u, 2) / (2 * o2));
            return (float)(a * b);
        }

        // u = mean
        // o = variance
        IEnumerable<(float Value, float Empiric, float Teoretic)> Distribution(IEnumerable<float> data, float u, float o2)
        {
            var freq = data.GroupBy(t => t)
                .ToDictionary(t => t.Key, t => t.Count());

            var total = (float)data.Count();
            foreach (var item in freq.OrderBy(t => t.Key))
            {
                var teoreticValue = NormalDistribution(item.Key, u, o2);
                yield return (item.Key, item.Value / total, teoreticValue);
            }
        }

        float Integral((float X, float Y) a, (float X, float Y) b)
        {
            var x = Math.Abs(b.X - a.X);
            var y = Math.Abs(b.Y - a.Y);

            return x * y;
        }

        // u = mean
        // o = variance
        IEnumerable<(float Value, float Empiric, float Teoretic)> CumulativeDistribution(IEnumerable<float> data, float u, float o2)
        {
            var freq = data.GroupBy(t => t)
                .ToDictionary(t => t.Key, t => t.Count())
                .OrderBy(t => t.Key)
                .ToList();

            var total = (float)data.Count();
            var teoreticValueSum = 0f;
            var empiricValueSum = 0f;

            var prevTeoreticValue = 0f;
            for (int i = 0; i < freq.Count - 1; i++)
            {
                var item = freq[i];
                var previousItem = freq[i + 1];
                empiricValueSum += item.Value;
                var teoreticValue = NormalDistribution(item.Key, u, o2);
                teoreticValueSum += Integral((previousItem.Key, prevTeoreticValue), (item.Key, teoreticValue));
                prevTeoreticValue = teoreticValue;
                yield return (item.Key, empiricValueSum / total, teoreticValueSum);
            }
        }

        Task ExportToCSV(string name, IEnumerable<(float Value, float Empiric, float Teoretic)> data)
        {
            return File.WriteAllLinesAsync(name, data.Select(t => $"{t.Value};{t.Empiric};{t.Teoretic}"));
        }

        static async Task Main(string[] args)
        {
            var p = new Program();
            var irisDataSet = (await p.LoadIrisDataAsync("iris.data.txt")).ToList();

            // mean
            var sepalLengthMedian = irisDataSet.Average(t => t.SepalLength);
            var sepalWidthMedian = irisDataSet.Average(t => t.SepalWidth);
            var petalLengthMedian = irisDataSet.Average(t => t.PetalLength);
            var petalWidthMedian = irisDataSet.Average(t => t.PetalWidth);

            // Rozptyl
            var sepalLengthVariance = irisDataSet.Variance(sepalLengthMedian, t => t.SepalLength);
            var sepalWidthVariance = irisDataSet.Variance(sepalWidthMedian, t => t.SepalWidth);
            var petalLengthVariance = irisDataSet.Variance(petalLengthMedian, t => t.PetalLength);
            var petalWidthVariance = irisDataSet.Variance(petalWidthMedian, t => t.PetalWidth);

            var sepalLengthDistribution = p.Distribution(irisDataSet.Select(t => t.SepalLength), sepalLengthMedian, sepalLengthVariance);
            var sepalWidthDistribution = p.Distribution(irisDataSet.Select(t => t.SepalWidth), sepalWidthMedian, sepalWidthVariance);
            var petalLengthDistribution = p.Distribution(irisDataSet.Select(t => t.PetalLength), petalLengthMedian, petalLengthVariance);
            var petalWidthDistribution = p.Distribution(irisDataSet.Select(t => t.PetalWidth), petalWidthMedian, petalWidthVariance);

            await Task.WhenAll(
                p.ExportToCSV($"{nameof(sepalLengthDistribution)}.csv", sepalLengthDistribution),
                p.ExportToCSV($"{nameof(sepalWidthDistribution)}.csv", sepalWidthDistribution),
                p.ExportToCSV($"{nameof(petalLengthDistribution)}.csv", petalLengthDistribution),
                p.ExportToCSV($"{nameof(petalWidthDistribution)}.csv", petalWidthDistribution)
                );
            var sepalLengthCumulativeDistribution = p.CumulativeDistribution(irisDataSet.Select(t => t.SepalLength), sepalLengthMedian, sepalLengthVariance);
            var sepalWidthCumulativeDistribution = p.CumulativeDistribution(irisDataSet.Select(t => t.SepalWidth), sepalWidthMedian, sepalWidthVariance);
            var petalLengthCumulativeDistribution = p.CumulativeDistribution(irisDataSet.Select(t => t.PetalLength), petalLengthMedian, petalLengthVariance);
            var petalWidthCumulativeDistribution = p.CumulativeDistribution(irisDataSet.Select(t => t.PetalWidth), petalWidthMedian, petalWidthVariance);

            await Task.WhenAll(
                p.ExportToCSV($"{nameof(sepalLengthCumulativeDistribution)}.csv", sepalLengthCumulativeDistribution),
                p.ExportToCSV($"{nameof(sepalWidthCumulativeDistribution)}.csv", sepalWidthCumulativeDistribution),
                p.ExportToCSV($"{nameof(petalLengthCumulativeDistribution)}.csv", petalLengthCumulativeDistribution),
                p.ExportToCSV($"{nameof(petalWidthCumulativeDistribution)}.csv", petalWidthCumulativeDistribution)
                );
        }
    }
}
