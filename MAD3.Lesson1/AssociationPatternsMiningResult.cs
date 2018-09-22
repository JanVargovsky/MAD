using Accord.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MAD3.Lesson1
{
    class AssociationPatternsMiningResult
    {
        readonly IList<int[]> _transactions;

        public int TransactionsCount => _transactions.Count;
        public float MinSupport { get; }
        public int FrequentItemsetsCount => Patterns.Count;

        // <Pattern, Transactions>
        public Dictionary<HashSet<int>, HashSet<int>> Patterns { get; set; }

        public AssociationPatternsMiningResult(IList<int[]> transactions, float minSupport)
        {
            _transactions = transactions;
            MinSupport = minSupport;
            Patterns = new Dictionary<HashSet<int>, HashSet<int>>(HashSet<int>.CreateSetComparer());
        }

        public float Support<T>(ICollection<T> transactions) => Support(transactions.Count);
        public float Support(int transactionCount) => transactionCount / (float)TransactionsCount;

        public IEnumerable<FrequentItemset> GetFrequentItemsets(int k)
        {
            var itemsets = Patterns.Where(t => t.Key.Count == k)
                .Select(t => new FrequentItemset(t.Key.OrderBy(a => a).ToList(), Support(t.Value)))
                .OrderBy(t => t);
            return itemsets;
        }

        public IEnumerable<ConfidenceResult> Confidence(KeyValuePair<HashSet<int>, HashSet<int>> pattern)
        {
            // cant generate any rule from set with one element
            if (pattern.Key.Count < 2)
                yield break;

            var suppXY = Support(pattern.Value);
            var values = pattern.Key.ToArray();
            for (int i = 1; i < values.Length; i++)
            {
                foreach (var item in Combinatorics.Combinations(values, i))
                {
                    var x = new HashSet<int>(item);
                    var y = new HashSet<int>(values);
                    y.ExceptWith(x);

                    Debug.Assert(x.Count + y.Count == values.Length);

                    float suppX;
                    if (Patterns.TryGetValue(x, out var xTransactions))
                        suppX = Support(xTransactions);
                    else
                    {
                        // Should not happen
                        var xTransactionCount = _transactions.Count(t => x.All(a => t.Contains(a)));
                        suppX = Support(xTransactionCount);
                    }
                    var confidence = suppXY / suppX;
                    yield return new ConfidenceResult(x, y, confidence);
                }
            }
        }
    }

    class FrequentItemset : IComparable<FrequentItemset>
    {
        public IReadOnlyList<int> Itemset { get; }
        public float Support { get; }

        public FrequentItemset(IReadOnlyList<int> items, float support)
        {
            Itemset = items;
            Support = support;
        }

        public int CompareTo(FrequentItemset other)
        {
            var supportCompare = other.Support.CompareTo(Support);
            if (supportCompare != 0)
                return supportCompare;

            var countCompare = Itemset.Count.CompareTo(other.Itemset.Count);
            if (countCompare != 0)
                return countCompare;

            for (int i = 0; i < Itemset.Count; i++)
            {
                var itemsetCompare = Itemset[i].CompareTo(other.Itemset[i]);
                if (itemsetCompare != 0)
                    return itemsetCompare;
            }
            return 0;
        }
    }

    class ConfidenceResult
    {
        public HashSet<int> X { get; }
        public HashSet<int> Y { get; }
        public float Confidence { get; }

        public ConfidenceResult(HashSet<int> x, HashSet<int> y, float confidence)
        {
            X = x;
            Y = y;
            Confidence = confidence;
        }
    }
}
