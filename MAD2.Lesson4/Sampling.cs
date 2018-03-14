using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD2.Lesson4
{
    class Sampling
    {
        readonly Random r = new Random();

        public Graph RandomNode(Graph g, double p)
        {
            var result = Graph.Empty;

            IEnumerable<int> GetNeighborhoodsOf(int nodeId)
            {
                foreach (var e in g.Edges)
                {
                    if (e.From == nodeId)
                        yield return e.To;
                    else if (e.To == nodeId)
                        yield return e.From;
                }
            }

            foreach (var node in g.Nodes)
            {
                var d = r.NextDouble();
                if (d <= p)
                {
                    result.Nodes.Add(node);

                    var neighborhoods = GetNeighborhoodsOf(node);

                    foreach (var neighborhood in neighborhoods)
                    {
                        result.Nodes.Add(neighborhood);
                        result.Edges.Add(new Edge(node, neighborhood));
                    }
                }
            }

            result.Nodes = result.Nodes.Distinct().ToList();

            return result;
        }

        public Graph RandomEdge(Graph g, double p)
        {
            var result = Graph.Empty;

            foreach (var e in g.Edges)
            {
                var d = r.NextDouble();
                if (d <= p)
                {
                    result.Edges.Add(e);
                }
            }

            result.Nodes.AddRange(result.Edges.SelectMany(e => new[] { e.From, e.To }).Distinct());

            return result;
        }

        // l = distance
        // s = number of snowballs
        public Graph Snowball(Graph g, int l, int s)
        {
            var result = Graph.Empty;

            IEnumerable<int> GetNeighborhoodsOf(int nodeId, int depth)
            {
                if (depth <= 0)
                    yield break;

                foreach (var e in g.Edges)
                {
                    if (e.From == nodeId)
                    {
                        yield return e.To;
                        foreach (var ee in GetNeighborhoodsOf(e.To, depth - 1))
                            yield return ee;
                    }
                    else if (e.To == nodeId)
                    {
                        yield return e.From;
                        foreach (var ee in GetNeighborhoodsOf(e.To, depth - 1))
                            yield return ee;
                    }
                }
            }

            int initialNode = g.Nodes[r.Next(g.Nodes.Count)]; // aka seed

            var neighborhoodsWithMaxDistanceL = GetNeighborhoodsOf(initialNode, l).ToList();
            result.Nodes.AddRange(neighborhoodsWithMaxDistanceL.Distinct());

            // my extension - add an extra snowball(s)
            for (int i = 0; i < s; i++)
            {
                initialNode = result.Nodes[r.Next(result.Nodes.Count)];
                neighborhoodsWithMaxDistanceL = GetNeighborhoodsOf(initialNode, l).ToList();
                result.Nodes.AddRange(neighborhoodsWithMaxDistanceL.Distinct());
            }

            result.Nodes = result.Nodes.Distinct().ToList();

            foreach (var from in result.Nodes)
                foreach (var to in result.Nodes)
                {
                    var edge = g.Edges.FirstOrDefault(t => t.From == from && t.To == to);
                    if (edge != null)
                        result.Edges.Add(edge);
                }

            return result;
        }
    }
}
