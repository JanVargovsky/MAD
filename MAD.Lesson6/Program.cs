using MoreLinq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson6
{
    public class Program
    {
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

        float EuclideanDistance<T>(T a, T b, params Func<T, float>[] attributeSelector)
        {
            return (float)Math.Sqrt(attributeSelector.Sum(t => Math.Pow(t(a) - t(b), 2)));
        }

        float CosineSimilarity<T>(T a, T b, params Func<T, float>[] attributeSelector)
        {
            var vectorOfA = attributeSelector.Select(t => t(a)).ToArray();
            var vectorOfB = attributeSelector.Select(t => t(b)).ToArray();
            return Enumerable.Range(0, attributeSelector.Length).Sum(t => vectorOfA[t] * vectorOfB[t]);
        }

        static async Task Main(string[] args)
        {
            var p = new Program();
            var irisDataSet = (await p.LoadIrisDataAsync("iris.data.txt")).ToList();

            // Prumer, median pro kazdy atribut
            var sepalLengthAverage = irisDataSet.Average(t => t.SepalLength);
            var sepalWidthAverage = irisDataSet.Average(t => t.SepalWidth);
            var petalLengthAverage = irisDataSet.Average(t => t.PetalLength);
            var petalWidthAverage = irisDataSet.Average(t => t.PetalWidth);

            var sepalLengthMedian = irisDataSet.Median(t => t.SepalLength);
            var sepalWidthMedian = irisDataSet.Median(t => t.SepalWidth);
            var petalLengthMedian = irisDataSet.Median(t => t.PetalLength);
            var petalWidthMedian = irisDataSet.Median(t => t.PetalWidth);

            // Rozptyl
            var sepalLengthVariance = irisDataSet.Variance(sepalLengthMedian, t => t.SepalLength);
            var sepalWidthVariance = irisDataSet.Variance(sepalWidthMedian, t => t.SepalWidth);
            var petalLengthVariance = irisDataSet.Variance(petalLengthMedian, t => t.PetalLength);
            var petalWidthVariance = irisDataSet.Variance(petalWidthMedian, t => t.PetalWidth);

            var irisVectorFuncs = new Func<IrisData, float>[]
            {
                d => d.SepalLength,
                d => d.SepalWidth,
                d => d.PetalLength,
                d => d.PetalWidth,
            };

            // Euklidovska vzdalenost/kosinova podobnost pro kazdou dvojici vektoru
            var euclideanDistances = MoreEnumerable.Cartesian(irisDataSet, irisDataSet, (a, b) => p.EuclideanDistance(a, b, irisVectorFuncs)).ToList();
            var cosinesSimilarities = MoreEnumerable.Cartesian(irisDataSet, irisDataSet, (a, b) => p.CosineSimilarity(a, b, irisVectorFuncs)).ToList();

            // Prumerny vektor
            var averageVector = new[] { sepalLengthAverage, sepalWidthAverage, petalLengthAverage, petalWidthAverage };
            var averageVectorAsObject = new IrisData
            {
                SepalLength = sepalLengthAverage,
                SepalWidth = sepalWidthAverage,
                PetalLength = petalLengthAverage,
                PetalWidth = petalWidthAverage,
            };

            // Rozptyl = odchylka od prumerneho vektoru?
            var euclideanVariances = irisDataSet.Select(t => p.EuclideanDistance(averageVectorAsObject, t, irisVectorFuncs)).ToList();
            var cosinesVariances = irisDataSet.Select(t => p.CosineSimilarity(averageVectorAsObject, t, irisVectorFuncs)).ToList();
        }
    }
}
