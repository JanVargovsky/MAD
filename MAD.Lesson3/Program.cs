using MoreLinq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson3
{
    class Program
    {
        public class Edge
        {
            public int From { get; set; }
            public int To { get; set; }
        }

        async Task<IEnumerable<Edge>> ParseCsvAsync(string file)
        {
            var lines = await File.ReadAllLinesAsync(file);
            Edge Parse(string line)
            {
                var tokens = line.Split(';');
                return new Edge
                {
                    From = int.Parse(tokens[0]),
                    To = int.Parse(tokens[1]),
                };
            }
            return lines.Select(Parse);
        }

        bool[,] ToIncidenceMatrix(IEnumerable<Edge> edges, int count)
        {
            var result = new bool[count, count];
            edges.ForEach(t => result[t.From, t.To] = true);
            return result;
        }

        int[] ToHistogram(bool[,] incidenceMatrix)
        {
            var result = new int[incidenceMatrix.GetLength(0)];
            for (int i = 0; i < incidenceMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < incidenceMatrix.GetLength(1); j++)
                {
                    if (incidenceMatrix[i, j])
                        result[i]++;
                }
            }
            return result;
        }

        static async Task Main(string[] args)
        {
            string FileName = Path.Combine(Environment.CurrentDirectory, "KarateClub.csv");
            var p = new Program();
            var edges = await p.ParseCsvAsync(FileName);

            Dictionary<int, int> degrees = new Dictionary<int, int>();
            edges.ForEach(t =>
            {
                degrees[t.From] = 0;
                degrees[t.To] = 0;
            });

            var matrix = p.ToIncidenceMatrix(edges, degrees.Count + 1);

            foreach (var edge in edges)
            {
                degrees[edge.From]++;
                degrees[edge.To]++;
            }

            var minDegree = degrees.MinBy(t => t.Value);
            var maxDegree = degrees.MaxBy(t => t.Value);
            var averageDegree = degrees.Average(t => t.Value);
            Console.WriteLine($"Min degree: id={minDegree.Key}, degree={minDegree.Value}");
            Console.WriteLine($"Max degree: id={maxDegree.Key}, degree={maxDegree.Value}");
            Console.WriteLine($"Average degree: {averageDegree:F2}");
            Console.WriteLine($"Histogram: ");
            var histogram = p.ToHistogram(matrix);
            Console.WriteLine(string.Join(Environment.NewLine, histogram.Select((t,i) => $"{i}={t}")));
        }
    }
}
