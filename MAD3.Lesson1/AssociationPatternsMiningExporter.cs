using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAD3.Lesson1
{
    class AssociationPatternsMiningExporter
    {
        public string ExportToString(string dataset, AssociationPatternsMiningResult result)
        {
            const int Length = 18;
            var sb = new StringBuilder();
            sb
                .AppendLine($"{"Dataset:".PadRight(Length)}{dataset}")
                .AppendLine($"{"Transactions:".PadRight(Length)}{result.TransactionsCount}")
                .AppendLine($"{"Min. Support:".PadRight(Length)}{result.MinSupport:f3}")
                .AppendLine($"{"Freq. Itemsets:".PadRight(Length)}{result.FrequentItemsetsCount}")
                .AppendLine($"{"Min. Confidence:".PadRight(Length)}{result.MinConfidence:f3}")
                .AppendLine($"{"Rules:".PadRight(Length)}{result.Rules.Count}");

            int length = 1;
            while (true)
            {
                var frequentItemsets = result.GetFrequentItemsets(length)
                    .Select(t => new
                    {
                        Itemsets = $"[{string.Join(", ", t.Itemset)}]",
                        t.Support
                    }).ToArray();

                if (!frequentItemsets.Any())
                    break;

                sb
                    .AppendLine()
                    .AppendLine($"length = {length} sup");

                
                int minPadding = frequentItemsets.Max(t => t.Itemsets.Length);

                foreach (var item in frequentItemsets.TakeWhile((_, index) => index < 10))
                {
                    sb
                        .Append(item.Itemsets.PadLeft(minPadding))
                        .Append("   ")
                        .Append(item.Support.ToString("f3"))
                        .AppendLine();
                }
                length++;
            }

            length = 2;
            while (true)
            {
                var rules = result.GetRules(length)
                    .Select(t => new
                    {
                        Rule = $"[{string.Join(", ", t.X.OrderBy(tt => tt))}]->{t.Y}",
                        t.Confidence
                    }).ToArray();

                if (!rules.Any())
                    break;

                sb
                    .AppendLine()
                    .AppendLine($"length = {length} conf");

                int minPadding = rules.Max(t => t.Rule.Length);

                foreach (var item in rules.TakeWhile((_, index) => index < 10))
                {
                    sb
                        .Append(item.Rule.PadLeft(minPadding))
                        .Append("   ")
                        .Append(item.Confidence.ToString("f3"))
                        .AppendLine();
                }
                length++;
            }

            return sb.ToString();
        }

        public Task ExportToFileAsync(string dataset, AssociationPatternsMiningResult result, string filename)
            => File.WriteAllTextAsync(filename, ExportToString(dataset, result));

        public void ExportToConsole(string dataset, AssociationPatternsMiningResult result)
            => Console.WriteLine(ExportToString(dataset, result));

        public string ExportToString(IEnumerable<Rule> confidences)
        {
            var sb = new StringBuilder();

            foreach (var confidence in confidences)
            {
                sb
                    .Append("conf({")
                    .AppendJoin(", ", confidence.X)
                    .Append("}")
                    .Append(" => ")
                    .Append("{")
                    .AppendJoin(", ", confidence.Y)
                    .Append("})")
                    .Append(" = ")
                    .Append(confidence.Confidence.ToString("f3"))
                    .AppendLine();
            }

            return sb.ToString();
        }

        public void ExportToConsole(IEnumerable<Rule> confidences)
            => Console.WriteLine(ExportToString(confidences));
    }
}
