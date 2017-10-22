using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson3
{
    public class Program
    {
        async Task<int[,]> LoadMatrixAsync(string file, int size)
        {
            var matrix = new int[size, size];

            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines)
            {
                var tokens = line.Split(';');
                var from = int.Parse(tokens[0]);
                var to = int.Parse(tokens[1]);

                matrix[from, to] = matrix[to, from] = 1;
            }

            return matrix;
        }

        int[,] FloydWarshall(int[,] inputMatrix)
        {
            int length = inputMatrix.GetLength(0);
            var matrix = new int[length, length];

            for (int i = 1; i < length; i++)
                for (int j = 1; j < length; j++)
                    matrix[i, j] = inputMatrix[i, j] != 0 ? inputMatrix[i, j] : 10000;

            for (int k = 1; k < length; k++)
                for (int i = 1; i < length; i++)
                    for (int j = 1; j < length; j++)
                    {
                        long newLength = matrix[i, k] + (long)matrix[k, j];
                        if (matrix[i, j] > newLength)
                        {
                            //Console.WriteLine($"[{i},{j}] New distance from {matrix[i, j]} to {newLength}");
                            matrix[i, j] = (int)newLength;
                        }
                    }

            return matrix;
        }

        double AverageDistance(int[,] matrix)
        {
            int length = matrix.GetLength(0);
            double total = 0;
            for (int i = 1; i < length; i++)
                for (int j = i + 1; j < length; j++)
                    total += matrix[i, j];

            var nodesCount = length - 1;
            var result = 2d / (nodesCount * (nodesCount - 1)) * total;
            return result;
        }

        int Average(int[,] matrix)
        {
            int length = matrix.GetLength(0);
            int result = 0;
            for (int i = 1; i < length; i++)
                for (int j = i + 1; j < length; j++)
                    if (matrix[i, j] > result)
                        result = matrix[i, j];

            return result;
        }

        (int Value, int Frequency, double Percentage)[] Frequency(int[,] matrix)
        {
            var frequency = new Dictionary<int, int>();
            int length = matrix.GetLength(0);

            for (int i = 1; i < length; i++)
                for (int j = i + 1; j < length; j++)
                {
                    int value = matrix[i, j];
                    if (frequency.ContainsKey(value))
                        frequency[value]++;
                    else
                        frequency[value] = 1;
                }

            double total = frequency.Values.Sum();
            return frequency
                .OrderBy(t => t.Key)
                .Select(t => (t.Key, t.Value, t.Value / total * 100))
                .ToArray();
        }

        double[] ClosenessCentrality(int[,] matrix)
        {
            var size = matrix.GetLength(0);
            var result = new double[size];
            int n = size - 1;
            for (int i = 1; i < size; i++)
            {
                for (int j = 1; j < size; j++)
                    if (i != j)
                        result[i] += matrix[i, j];

                result[i] = n / result[i];
            }
            return result;
        }

        public void WriteAll(int[,] incidenceMatrix)
        {
            var floydMatrix = FloydWarshall(incidenceMatrix);

            Console.WriteLine($"Prumerna vzdalenost: {AverageDistance(floydMatrix)}");
            Console.WriteLine($"Prumer: {Average(floydMatrix)}");

            Console.WriteLine("Četnost");
            var frequency = Frequency(floydMatrix);
            foreach (var f in frequency)
                Console.WriteLine($"{f.Value}={f.Frequency} ({f.Percentage:n2}%)");

            Console.WriteLine("Closeness centrality");
            var closenessCentrality = ClosenessCentrality(floydMatrix);
            for (int i = 1; i < closenessCentrality.Length; i++)
                Console.WriteLine($"{i}={closenessCentrality[i]:n4}");
        }

        async static Task Main(string[] args)
        {
            const string Filename = "KarateClub.csv";
            var p = new Program();
            var matrix = await p.LoadMatrixAsync(Filename, 34 + 1);
            var floydMatrix = p.FloydWarshall(matrix);

            Console.WriteLine($"Prumerna vzdalenost: {p.AverageDistance(floydMatrix)}");
            Console.WriteLine($"Prumer: {p.Average(floydMatrix)}");

            Console.WriteLine("Četnost");
            var frequency = p.Frequency(floydMatrix);
            foreach (var f in frequency)
                Console.WriteLine($"{f.Value}={f.Frequency} ({f.Percentage:n2}%)");

            Console.WriteLine("Closeness centrality");
            var closenessCentrality = p.ClosenessCentrality(floydMatrix);
            for (int i = 1; i < closenessCentrality.Length; i++)
                Console.WriteLine($"{i}={closenessCentrality[i]:n4}");
        }
    }
}
