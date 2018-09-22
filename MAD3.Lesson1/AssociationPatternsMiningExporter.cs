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
                .AppendLine($"{"Min. Support:".PadRight(Length)}{result.MinSupport}")
                .AppendLine($"{"Freq. Itemsets:".PadRight(Length)}{result.FrequentItemsetsCount}");

            int length = 1;
            while (true)
            {
                var frequentItemsets = result.GetFrequentItemsets(length)
                    .Select(t => new
                    {
                        Itemsets = $"[{string.Join(", ", t.Itemset)}]",
                        t.Support
                    }).ToList();

                if (!frequentItemsets.Any())
                    break;

                sb
                    .AppendLine()
                    .AppendLine($"length = {length}");

                
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

            return sb.ToString();
        }

        public Task ExportToFileAsync(string dataset, AssociationPatternsMiningResult result, string filename)
            => File.WriteAllTextAsync(filename, ExportToString(dataset, result));

        public void ExportToConsole(string dataset, AssociationPatternsMiningResult result)
            => Console.WriteLine(ExportToString(dataset, result));

        public string ExportToString(IEnumerable<ConfidenceResult> confidences)
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

        public void ExportToConsole(IEnumerable<ConfidenceResult> confidences)
            => Console.WriteLine(ExportToString(confidences));
    }
}
