using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson2
{
    public class Program
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

        int[,] ToIncidenceMatrix(IEnumerable<Edge> edges, int count)
        {
            var result = new int[count, count];
            edges.ForEach(t => result[t.From, t.To] = result[t.To, t.From] = 1);
            return result;
        }

        int GetDegree(int[,] incidenceMatrix, int id)
        {
            int degree = 0;
            for (int i = 0; i < incidenceMatrix.GetLength(0); i++)
                if (incidenceMatrix[id, i] > 0)
                    degree++;
            return degree;
        }

        (int Degree, int Count)[] ToHistogram(Dictionary<int, int> degrees) => degrees.Values
            .GroupBy(t => t)
            .OrderBy(t => t.Key)
            .Select(t => (t.Key, t.Count()))
            .ToArray();

        void ExportHistogramToCsv((int Degree, int Count)[] histogram)
        {
            File.WriteAllLines("histogram.csv", histogram.Select(t => $"{t.Degree},{t.Count}"));
        }

        public void WriteAll(int[,] matrix)
        {
            Dictionary<int, int> degrees = new Dictionary<int, int>(); // <nodeId, degree>
            for (int i = 1; i < matrix.GetLength(0); i++)
                degrees[i] = GetDegree(matrix, i);

            var minDegree = degrees.MinBy(t => t.Value);
            var maxDegree = degrees.MaxBy(t => t.Value);
            var averageDegree = degrees.Average(t => t.Value);
            Console.WriteLine($"Min degree: id={minDegree.Key}, degree={minDegree.Value}");
            Console.WriteLine($"Max degree: id={maxDegree.Key}, degree={maxDegree.Value}");
            Console.WriteLine($"Average degree: {averageDegree:F2}");
            Console.WriteLine($"Histogram: ");
            var histogram = ToHistogram(degrees);
            Console.WriteLine(string.Join(Environment.NewLine, histogram.Select(t => $"Degree={t.Degree}, count={t.Count}")));
        }

        static async Task Main(string[] args)
        {
            string FileName = Path.Combine(Environment.CurrentDirectory, "KarateClub.csv");
            var p = new Program();
            var edges = await p.ParseCsvAsync(FileName);

            Dictionary<int, int> degrees = new Dictionary<int, int>(); // <nodeId, degree>
            var matrix = p.ToIncidenceMatrix(edges, 35);
            for (int i = 1; i < matrix.GetLength(0); i++)
                degrees[i] = p.GetDegree(matrix, i);

            var minDegree = degrees.MinBy(t => t.Value);
            var maxDegree = degrees.MaxBy(t => t.Value);
            var averageDegree = degrees.Average(t => t.Value);
            Console.WriteLine($"Min degree: id={minDegree.Key}, degree={minDegree.Value}");
            Console.WriteLine($"Max degree: id={maxDegree.Key}, degree={maxDegree.Value}");
            Console.WriteLine($"Average degree: {averageDegree:F2}");
            Console.WriteLine($"Histogram: ");
            var histogram = p.ToHistogram(degrees);
            Console.WriteLine(string.Join(Environment.NewLine, histogram.Select(t => $"Degree={t.Degree}, count={t.Count}")));
            p.ExportHistogramToCsv(histogram);
        }
    }
}
