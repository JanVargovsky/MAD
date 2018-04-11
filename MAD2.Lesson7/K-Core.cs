using MoreLinq;
using System.Collections.Generic;
using System.Linq;

namespace MAD2.Lesson7
{
    public class KCore
    {
        public List<int> Compute(Graph m, int k)
        {
            Graph g = m.Copy();

            bool RemoveNodes()
            {
                var nodesToRemove = g.Nodes.Where(n => g.Degree(n) < k).ToArray();

                nodesToRemove.ForEach(t => g.Remove(t));
                //Console.WriteLine($"Removed nodes: [{string.Join(", ", nodesToRemove)}]");
                return nodesToRemove.Length > 0;
            }

            while (RemoveNodes()) ;
            return g.Nodes;
        }

        public List<int>[] ComputeAll(Graph g, int k)
        {
            var cores = Enumerable.Range(1, k).Select(t => Compute(g, t)).ToArray();

            for (int i = cores.Length - 1; i >= 0; i--)
                for (int j = 0; j < i; j++)
                    cores[j].RemoveAll(t => cores[i].Contains(t));

            return cores;
        }
    }
}
