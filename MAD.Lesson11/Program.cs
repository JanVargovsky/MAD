using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MAD.Lesson11
{
    public class Node
    {
        public Node(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Edge
    {
        public Edge(int from, int to)
        {
            From = from;
            To = to;
        }

        public int From { get; set; }
        public int To { get; set; }
    }

    public class Program
    {
        const string Filename = "actorsandmoviesWithGere.net.txt";

        public int[,] ToMatrix(Edge[] edges, int size, Func<Edge, int> edgeASelector, Func<Edge, int> edgeBSelector)
        {
            int[,] result = new int[size, size];

            foreach (var e1 in edges)
                foreach (var e2 in edges)
                {
                    if (e1 == e2)
                        continue;

                    var e1b = edgeBSelector(e1);
                    var e2b = edgeBSelector(e2);

                    // maji spolecny vrchol pres 2 disjunktni hrany?
                    if (e1b == e2b)
                    {
                        var e1a = edgeASelector(e1);
                        var e2a = edgeASelector(e2);

                        result[e1a, e2a]++;
                        //result[e2a, e1a]++;
                    }
                }

            return result;
        }

        async Task<(Dictionary<int, Node> Actors, Dictionary<int, Node> Movies, Edge[] Edges)> LoadActorsAndMoviesAsync(string filename)
        {
            int total, actorsCount, moviesCount;
            using (var sr = new StreamReader(filename))
            {
                var line = await sr.ReadLineAsync();
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                total = int.Parse(tokens[0]);
                actorsCount = int.Parse(tokens[1]);
                moviesCount = total - actorsCount;

                var actors = new Dictionary<int, Node>();
                var movies = new Dictionary<int, Node>();

                async Task<Node> ReadNodeAsync()
                {
                    line = await sr.ReadLineAsync();
                    tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return new Node(int.Parse(tokens[0]), tokens[1]);
                }

                for (int i = 0; i < actorsCount; i++)
                {
                    var node = await ReadNodeAsync();
                    actors[node.Id] = node;
                }
                for (int i = 0; i < moviesCount; i++)
                {
                    var node = await ReadNodeAsync();
                    movies[node.Id] = node;
                }

                async Task<Edge> ReadEdgeAsync()
                {
                    line = await sr.ReadLineAsync();
                    if (line == null)
                        return null;
                    tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return new Edge(int.Parse(tokens[0]), int.Parse(tokens[1]));
                }

                var edges = new List<Edge>();
                while (true)
                {
                    var edge = await ReadEdgeAsync();
                    if (edge == null)
                        break;
                    edges.Add(edge);
                }

                return (actors, movies, edges.ToArray());
            }
        }

        void PrintMatrix(int[,] matrix)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                for (int x = 0; x < matrix.GetLength(0); x++)
                    Console.Write($"{matrix[x, y]} ");
                Console.WriteLine();
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                var p = new Program();

                //var numbers = new[]
                //{
                //    new Node(1, "1"),
                //    new Node(2, "2"),
                //};
                //var chars = new[]
                //{
                //    new Node(1, "a"),
                //    new Node(2, "b"),
                //    new Node(3, "c"),
                //};
                //var numbersCharsEdges = new[]
                //{
                //    new Edge(1, 1),
                //    new Edge(1, 2),
                //    new Edge(2, 1),
                //    new Edge(2, 2),
                //    new Edge(2, 3),
                //};
                //var numbersMatrix = p.ToMatrix(numbersCharsEdges, numbers.Length, e => e.From - 1, e => e.To);
                //var charsMatrix = p.ToMatrix(numbersCharsEdges, chars.Length, e => e.To - 1, e => e.From);
                //Console.WriteLine("Numbers matrix");
                //p.PrintMatrix(numbersMatrix);
                //Console.WriteLine("Chars matrix");
                //p.PrintMatrix(charsMatrix);

                void WriteInfo(int[,] matrix, string name)
                {
                    Console.WriteLine(name);
                    p.PrintMatrix(matrix);
                    new Lesson2.Program().WriteAll(matrix, 0);
                    new Lesson3.Program().WriteAll(matrix, 0);
                    new Lesson4.Program().WriteAll(matrix, 0);
                }

                var (actors, movies, edges) = await p.LoadActorsAndMoviesAsync(Filename);
                var actorsMatrix = p.ToMatrix(edges, actors.Count, e => e.From - 1, e => e.To);
                var moviesMatrix = p.ToMatrix(edges, movies.Count, e => e.To - actors.Count - 1, e => e.From);
                WriteInfo(actorsMatrix, "Actors matrix");
                WriteInfo(moviesMatrix, "Movies matrix");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
