using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD3.Lesson3
{
    public class ClusteringConditionArgs
    {
        public int Clusters { get; }
        public double Height { get; }

        public ClusteringConditionArgs(int numberOfClusters, double height)
        {
            Clusters = numberOfClusters;
            Height = height;
        }
    }

    public class HierarchicalAgglomerativeClustering
    {
        (DistanceMatrix, List<List<int>>) Iteration(DistanceMatrix distanceMatrix, List<List<int>> clusters, Func<double, double, double> takeFunc)
        {
            var newDistanceMatrix = new DistanceMatrix(distanceMatrix.Size - 1);
            var (minRow, minCol) = distanceMatrix.Min();

            for (int oldRow = 0, newRow = 1; oldRow < distanceMatrix.Size; oldRow++)
            {
                if (oldRow == minRow || oldRow == minCol) continue;

                for (int oldCol = 0, newCol = 1; oldCol < oldRow; oldCol++)
                {
                    if (oldCol == minCol || oldCol == minRow) continue;

                    var oldValue = distanceMatrix[oldRow, oldCol];
                    newDistanceMatrix[newRow, newCol] = oldValue;

                    newCol++;
                }

                newRow++;
            }

            for (int i = 0, row = 1; i < distanceMatrix.Size; i++)
            {
                if (i == minRow || i == minCol) continue;

                var a = i > minRow ? distanceMatrix[i, minRow] : distanceMatrix[minRow, i];
                var b = i > minCol ? distanceMatrix[i, minCol] : distanceMatrix[minCol, i];
                var value = takeFunc(a, b);
                newDistanceMatrix[row++, 0] = value;
            }

            var newClusters = new List<List<int>>();
            var newCluster = new List<int>();
            newCluster.AddRange(clusters[minRow]);
            newCluster.AddRange(clusters[minCol]);
            newClusters.Add(newCluster);

            for (int i = 0; i < clusters.Count; i++)
            {
                if (i == minRow || i == minCol) continue;
                newClusters.Add(clusters[i]);
            }

            return (newDistanceMatrix, newClusters);
        }

        internal (DistanceMatrix, List<List<int>>) SingleLinkageIteration(DistanceMatrix distanceMatrix, List<List<int>> clusters)
            => Iteration(distanceMatrix, clusters, Math.Min);

        internal (DistanceMatrix, List<List<int>>) CompleteLinkageIteration(DistanceMatrix distanceMatrix, List<List<int>> clusters)
            => Iteration(distanceMatrix, clusters, Math.Max);

        internal List<List<int>> CreateClusters(int size) =>
            Enumerable.Range(0, size)
                .Select(t => new List<int> { t })
                .ToList();

        ClusteringConditionArgs CreateConditionArgs(DistanceMatrix distanceMatrix, List<List<int>> clusters)
        {
            var (row, col) = distanceMatrix.Min();
            var height = distanceMatrix[row, col];
            return new ClusteringConditionArgs(clusters.Count, height);
        }

        public List<List<int>> SingleLinkage(DistanceMatrix distanceMatrix, Func<ClusteringConditionArgs, bool> endCondition)
        {
            var clusters = CreateClusters(distanceMatrix.Size);
            while (distanceMatrix.Size > 1 && endCondition(CreateConditionArgs(distanceMatrix, clusters)))
                (distanceMatrix, clusters) = SingleLinkageIteration(distanceMatrix, clusters);
            return clusters;
        }

        public List<List<int>> CompleteLinkage(DistanceMatrix distanceMatrix, Func<ClusteringConditionArgs, bool> endCondition)
        {
            var clusters = CreateClusters(distanceMatrix.Size);
            while (distanceMatrix.Size > 1 && endCondition(CreateConditionArgs(distanceMatrix, clusters)))
                (distanceMatrix, clusters) = CompleteLinkageIteration(distanceMatrix, clusters);
            return clusters;
        }
    }
}
