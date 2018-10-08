using Accord.Math;
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

            public static EntropyKey CreateFrom(GridBlock[] blocks, IDataset dataset, int row)
            {
                var indexes = new int[blocks.Length];
                for (int i = 0; i < indexes.Length; i++)
                    indexes[i] = blocks[i].GetBlock(dataset[row, i]);

                return new EntropyKey(indexes);
            }

            public override string ToString() => $"[{string.Join(", ", Indexes)}]";
        }

        double Entropy(double pi) => pi * Math.Log(pi, 2);

        public double Entropy(IDataset dataset, int m)
        {
            var gridBlocks = new GridBlock[dataset.NumberOfAttributes];
            for (int i = 0; i < gridBlocks.Length; i++)
                gridBlocks[i] = new GridBlock(dataset.MinOf(i), dataset.MaxOf(i), m);

            var grid = new Dictionary<EntropyKey, uint>();

            for (int row = 0; row < dataset.Count; row++)
            {
                var key = EntropyKey.CreateFrom(gridBlocks, dataset, row);

                if (!grid.ContainsKey(key))
                    grid[key] = 0;

                grid[key]++;
            }

            double entropy = 0d;

            foreach (var item in grid.Values)
            {
                var pi = item / (double)dataset.Count;
                var e = Entropy(pi);
                entropy += e;
            }

            return -entropy;
        }

        public MinimalEntropyResult GetMinimalEntropy(ColumnDataset dataset, int m)
        {
            double originalEntropy = Entropy(dataset, m);
            double minEntropy = double.MaxValue;
            ColumnDataset minDataset = null;

            foreach (var columns in Combinatorics.Combinations(dataset.Columns))
            {
                var newDataset = new ColumnDataset(columns);
                var newEntropy = Entropy(newDataset, m);

                //Console.WriteLine(entropy);
                if (newEntropy < minEntropy ||
                    (newEntropy == minEntropy && newDataset.NumberOfAttributes < minDataset.NumberOfAttributes))
                {
                    minEntropy = newEntropy;
                    minDataset = newDataset;
                }
            }

            return new MinimalEntropyResult(dataset, originalEntropy, minDataset, minEntropy);
        }
    }

    public class MinimalEntropyResult
    {
        public IDataset OriginalDataset { get; }
        public double OriginalDatasetEntropy { get; }
        public IDataset MinimizedDataset { get; }
        public double MinimizedDatasetEntropy { get; }

        public MinimalEntropyResult(IDataset originalDataset, double originalDatasetEntropy, IDataset minimizedDataset, double minimizedDatasetEntropy)
        {
            OriginalDataset = originalDataset;
            OriginalDatasetEntropy = originalDatasetEntropy;
            MinimizedDataset = minimizedDataset;
            MinimizedDatasetEntropy = minimizedDatasetEntropy;
        }
    }
}
