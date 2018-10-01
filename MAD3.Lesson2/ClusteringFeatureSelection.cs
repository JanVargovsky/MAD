using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD3.Lesson2
{
    public class ClusteringFeatureSelection
    {
        class EntropyKey : IEquatable<EntropyKey>
        {
            public int[] Indexes { get; }

            public EntropyKey(int[] indexes)
            {
                Indexes = indexes;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as EntropyKey);
            }

            public bool Equals(EntropyKey other)
            {
                return other != null &&
                       Indexes.SequenceEqual(other.Indexes);
            }

            public override int GetHashCode()
            {
                return Indexes.Sum();
            }

            public static EntropyKey CreateFrom(double[] values, GridBlock[] blocks)
            {
                var indexes = new int[values.Length];
                for (int i = 0; i < indexes.Length; i++)
                    indexes[i] = blocks[i].GetBlock(values[i]);

                return new EntropyKey(indexes);
            }

            public override string ToString() => $"[{string.Join(", ", Indexes)}]";
        }

        const int Base = 2;
        double Entropy(double pi) => pi * Math.Log(pi, Base) + (1 - pi) * Math.Log(1 - pi, Base);

        public double Entropy(Dataset dataset, int m)
        {
            var gridBlocks = new GridBlock[dataset.NumberOfAttributes];
            for (int i = 0; i < gridBlocks.Length; i++)
                gridBlocks[i] = new GridBlock(dataset.MinOf(i), dataset.MaxOf(i), m);

            var grid = new Dictionary<EntropyKey, uint>();

            foreach (var item in dataset.Data)
            {
                var key = EntropyKey.CreateFrom(item, gridBlocks);

                if (!grid.ContainsKey(key))
                    grid[key] = 0;

                grid[key]++;
            }

            double e = 0d;

            foreach (var item in grid.Values)
            {
                var pi = item / (double)dataset.Data.Count;
                var ee = Entropy(pi);
                e += ee;
            }

            return e;
        }
    }
}
