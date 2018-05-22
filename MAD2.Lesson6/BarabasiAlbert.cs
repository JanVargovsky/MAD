using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD2.Lesson6
{
    public class BarabasiAlbert
    {
        readonly Random r;

        public BarabasiAlbert()
        {
            r = new Random(42);
        }

        public Graph Basic(Graph g, int m, int t)
        {
            var result = Graph.Empty;
            int id = g.Nodes.Max() + 1;
            var nodes = g.Edges.SelectMany(e => new[] { e.From, e.To }).ToList();

            while (t-- > 0)
            {
                HashSet<int> nodeIdsFrom = new HashSet<int>();
                while (nodeIdsFrom.Count < m)
                {
                    var index = nodes[r.Next(nodes.Count)];
                    nodeIdsFrom.Add(index);
                }

                foreach (var nodeId in nodeIdsFrom)
                {
                    nodes.Add(nodeId);
                    result.Edges.Add(new Edge(nodeId, id));
                }
                result.Nodes.Add(id);
                nodes.Add(id);
                id++;
            }
            return result;
        }

        public enum InternalLinksStrategy
        {
            DoublePreferentialAttachment,
            RandomAttachment
        }

        public Graph InternalLinks(Graph g, int m, int t, int n, InternalLinksStrategy linksStrategy)
        {
            var result = Graph.Empty;
            int id = g.Nodes.Max() + 1;
            var nodes = g.Edges.SelectMany(e => new[] { e.From, e.To }).ToList();

            int GetRandomNode() => nodes[r.Next(nodes.Count)];
            int GetRandomNodeWithStrategy()
            {
                if (linksStrategy == InternalLinksStrategy.DoublePreferentialAttachment)
                    return nodes[r.Next(nodes.Count)];
                else
                    return g.Nodes[r.Next(g.Nodes.Count)];
            }

            while (t-- > 0)
            {
                HashSet<int> nodeIdsFrom = new HashSet<int>();
                while (nodeIdsFrom.Count < m)
                {
                    var index = GetRandomNode();
                    nodeIdsFrom.Add(index);
                }

                for (int i = 0; i < n; i++)
                {
                    // multi edges and self loops are off
                    while (true)
                    {
                        var (from, to) = (GetRandomNodeWithStrategy(), GetRandomNodeWithStrategy());
                        if (from == to) continue;

                        var edge = new Edge(from, to);
                        if (!result.Edges.Contains(edge))
                        {
                            result.Edges.Add(edge);
                            break;
                        }
                    }                    
                }

                foreach (var nodeId in nodeIdsFrom)
                {
                    nodes.Add(nodeId);
                    result.Edges.Add(new Edge(nodeId, id));
                }
                result.Nodes.Add(id);
                nodes.Add(id);
                id++;
            }
            return result;
        }
    }
}
