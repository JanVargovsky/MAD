using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson7
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

        [DebuggerDisplay("C={Centroid}, Count={Data.Count}")]
        class Cluster<T>
        {
            public T Centroid { get; set; }
            public List<T> Data { get; }

            public Cluster(T centroid)
            {
                Centroid = centroid;
                Data = new List<T>();
            }
        }

        IrisData Mean(IEnumerable<IrisData> data) => new IrisData
        {
            PetalLength = data.Average(t => t.PetalLength),
            PetalWidth = data.Average(t => t.PetalWidth),
            SepalLength = data.Average(t => t.SepalLength),
            SepalWidth = data.Average(t => t.SepalWidth),
        };

        bool AlmostEquals(IrisData a, IrisData b)
        {
            return a.PetalLength == b.PetalLength &&
                a.PetalWidth == b.PetalWidth &&
                a.SepalLength == b.SepalLength &&
                a.SepalWidth == b.SepalWidth;
        }

        float EuclideanDistance<T>(T a, T b, params Func<T, float>[] attributeSelector)
        {
            return (float)Math.Sqrt(attributeSelector.Sum(t => Math.Pow(t(a) - t(b), 2)));
        }

        IEnumerable<Cluster<IrisData>> KMeansClustering(ICollection<IrisData> data, int k)
        {
            var irisVectorFuncs = new Func<IrisData, float>[]
            {
                d => d.SepalLength,
                d => d.SepalWidth,
                d => d.PetalLength,
                d => d.PetalWidth,
            };

            var clusters = data
                .RandomSubset(k)
                .Select(t => new Cluster<IrisData>(t))
                .ToArray();

            void AddToNearestCluster(Cluster<IrisData>[] c, IrisData d)
            {
                var distances = c.Select(t => new { Cluster = t, Distance = EuclideanDistance(t.Centroid, d, irisVectorFuncs) }).ToList();
                var min = distances.MinBy(t => t.Distance);
                min.Cluster.Data.Add(d);
            }

            // Initial
            data.ForEach(t => AddToNearestCluster(clusters, t));
            bool nonEqual;
            do
            {
                var newClusters = clusters.Select(t => new Cluster<IrisData>(Mean(t.Data))).ToArray();
                data.ForEach(t => AddToNearestCluster(newClusters, t));


                nonEqual = false;
                for (int i = 0; i < k; i++)
                    if (!AlmostEquals(clusters[i].Centroid, newClusters[i].Centroid))
                    {
                        nonEqual = true;
                        break;
                    }

                clusters = newClusters;
            } while (nonEqual);

            return clusters.OrderByDescending(t => t.Data.Count);
        }

        void PrintClusters(IEnumerable<Cluster<IrisData>> clusters)
        {
            string IrisDataToString(IrisData d) => $"[{d.PetalLength}, {d.PetalWidth}, {d.SepalLength}, {d.SepalWidth}]";
            string ClusterToString(Cluster<IrisData> d) => $"Count={d.Data.Count}, Centroid={IrisDataToString(d.Centroid)}";

            Console.WriteLine($"k={clusters.Count()}");
            clusters.ForEach(c => Console.WriteLine(ClusterToString(c)));
            Console.WriteLine();
        }

        static async Task Main(string[] args)
        {
            var p = new Program();
            var irisDataSet = (await p.LoadIrisDataAsync("iris.data.txt")).ToList();


            for (int k = 2; k <= 5; k++)
            {
                var clusters = p.KMeansClustering(irisDataSet, k);
                p.PrintClusters(clusters);
            }
        }
    }
}
