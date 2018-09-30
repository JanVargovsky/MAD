using Accord.Math;
using System.Collections.Generic;
using System.Linq;

namespace MAD3.Lesson1
{
    class AssociationPatternsMining
    {
        readonly bool _ignorePatternsWith100;

        public AssociationPatternsMining(bool ignorePatternsWith100)
        {
            _ignorePatternsWith100 = ignorePatternsWith100;
        }

        public AssociationPatternsMiningResult VerticalCountingMethods(IList<int[]> data, float minSupport, float minConfidence)
        {
            var result = new AssociationPatternsMiningResult(data, minSupport, minConfidence);

            bool IsSupportOK(HashSet<int> rows)
            {
                var support = result.Support(rows);
                if (_ignorePatternsWith100 && support >= 1f)
                    return false;

                return support >= minSupport;
            }

            void AddPatternsToResult(IEnumerable<KeyValuePair<HashSet<int>, HashSet<int>>> items)
            {
                foreach (var item in items)
                    result.Patterns.Add(item.Key, item.Value);
            }

            Dictionary<HashSet<int>, HashSet<int>> GetF1()
            {
                // <Attribute, Rows>
                var F1 = new Dictionary<int, HashSet<int>>();
                for (int row = 0; row < data.Count; row++)
                {
                    foreach (var attribute in data[row])
                    {
                        if (!F1.TryGetValue(attribute, out var rows))
                            F1[attribute] = rows = new HashSet<int>();

                        rows.Add(row + 1);
                    }
                }

                return F1
                    .Where(t => IsSupportOK(t.Value))
                    .ToDictionary(
                    t => new HashSet<int> { t.Key }, 
                    t => t.Value);
            }

            var F = GetF1();
            AddPatternsToResult(F);

            int k = 2;
            while (F.Count >= 2)
            {
                var newF = new Dictionary<HashSet<int>, HashSet<int>>(HashSet<int>.CreateSetComparer());
                foreach (var ck in Combinatorics.Combinations(F.ToArray(), 2))
                {
                    var newPattern = new HashSet<int>(ck[0].Key);
                    newPattern.UnionWith(ck[1].Key);
                    if (newPattern.Count != k)
                        continue;
                    if (newF.ContainsKey(newPattern))
                        continue;

                    var newRows = new HashSet<int>(ck[0].Value);
                    newRows.IntersectWith(ck[1].Value);
                    if (!IsSupportOK(newRows))
                        continue;

                    newF[newPattern] = newRows;
                }

                AddPatternsToResult(newF);
                F = newF;
                k++;
            }
            result.CalculateRules();
            return result;
        }
    }
}
