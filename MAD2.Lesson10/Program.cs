using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD2.Lesson10
{
    class Program
    {
        async Task<(int[] Nodes, Matrix<int>[] Matrices)> LoadTemporaryNetworksAsync(string filename, int timeSplit)
        {
            (int TimeStamp, int From, int To) ParseLine(string line)
            {
                var values = line.Split('\t')
                    .Select(int.Parse)
                    .ToArray();
                return (values[0], values[1], values[2]);
            }

            Matrix<int> GroupToMatrix(IEnumerable<(int TimeStamp, int From, int To)> values)
            {
                Matrix<int> matrix = new Matrix<int>();
                values.ForEach(t => matrix.AddEdge(t.From, t.To));
                return matrix;
            }

            var data = (await File.ReadAllLinesAsync(filename))
                .Select(ParseLine)
                .ToArray();

            var nodes = data.Select(t => t.From).Concat(data.Select(t => t.To)).Distinct().ToArray();

            var matrices = data.GroupBy(t => t.TimeStamp / timeSplit)
                .Select(GroupToMatrix)
                .ToArray();
            return (nodes, matrices);
        }

        IDictionary<int, double> ClosenessCentrality(int[] nodes, Matrix<int> matrix)
        {
            double N = nodes.Length;
            var result = new Dictionary<int, double>();
            foreach (var node in nodes)
            {
                var missingEdges = matrix.NumberOfEdges(node);
                var value = missingEdges == 0 ? N : N / missingEdges;
                result[node] = value;
            }
            return result;
        }

        static async Task Main(string[] args)
        {
            const string Filename = "../../../../Datasets/ht09_contact_list.dat";
            const int TimeSplit = 60 * 60 * 24;
            var p = new Program();

            var (nodes, matrices) = await p.LoadTemporaryNetworksAsync(Filename, TimeSplit);

            for (int i = 0; i < matrices.Length; i++)
            {
                Console.WriteLine($"Timestamp = {i*TimeSplit} - {(i + 1) * TimeSplit}");
                var closeness = p.ClosenessCentrality(nodes, matrices[i]);

                Console.WriteLine(string.Join(Environment.NewLine, 
                    closeness.OrderBy(t => t.Value)
                    .Select(t => $"Node={t.Key}, Closeness={t.Value}")));
            }
        }
    }
}
