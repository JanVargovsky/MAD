using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class TextReport
    {
        public async Task WriteNaiveBayesProbabilitiesTableAsync(TextWriter tw, NaiveBayes nb)
        {
            await tw.WriteLineAsync(';' + string.Join(";", nb.Classes.Select(t => $"class='{t}'")));

            foreach (var attribute in nb.ConditionalProbabilities)
                foreach (var attributeValue in attribute.Value)
                {
                    await tw.WriteAsync($"{attribute.Key}='{attributeValue.Key}'");
                    foreach (var @class in nb.Classes)
                    {
                        if (attributeValue.Value.TryGetValue(@class, out var value))
                            await tw.WriteAsync($";{value:n2}");
                        else
                            await tw.WriteAsync($";0");
                    }
                    await tw.WriteLineAsync();
                }

            await tw.WriteLineAsync("Prior Probability;" + string.Join(";", nb.Classes.Select(t => $"{ nb.PriorProbabilities[t].Prior:n2}")));
        }

        public async Task WriteReport(string filename, DataMatrix allData, DataMatrix trainingData, DataMatrix predictData, List<string> predictResults, 
            string response, string[] predictors, NaiveBayes naiveBayes, float predictSuccess)
        {
            using (var sw = new StreamWriter("report.txt"))
            {
                await sw.WriteLineAsync($"Počet vstupních dat: {allData.RowsCount}");
                await sw.WriteLineAsync($"Počet vstupních trénovacích dat: {trainingData.RowsCount}");
                await sw.WriteLineAsync($"Počet vstupních predikovanýc dat: {predictData.RowsCount}");
                await sw.WriteLineAsync();

                await sw.WriteLineAsync("Atributy:");
                await sw.WriteLineAsync(@"Date_time - standardized date and time of sighting");
                await sw.WriteLineAsync("city - location of UFO sighting");
                await sw.WriteLineAsync("state/province - the US state or Canadian province, appears blank for other locations");
                await sw.WriteLineAsync("country - Country of UFO sighting");
                await sw.WriteLineAsync("UFO_shape - a one word description of the \"spacecraft\"");
                await sw.WriteLineAsync("length_of_encounter_seconds - standardized to seconds, length of the observation of the UFO");
                await sw.WriteLineAsync("described_duration _of_encounter - raw description of the length of the encounter(shows uncertainty to previous column)");
                await sw.WriteLineAsync("description - text description of the UFO encounter.Warning column is messy, with some curation it could lend itself to some natural language processing and await sw.WriteLineAsync analysis.");
                await sw.WriteLineAsync("date_documented - when was the UFO sighting reported");
                await sw.WriteLineAsync("latitude - latitude");
                await sw.WriteLineAsync("longitude - longitude");
                await sw.WriteLineAsync();

                await sw.WriteLineAsync($"Klasifikace pomoci Naive-Bayes ({response} -> {string.Join(" + ", predictors)})");
                await WriteNaiveBayesProbabilitiesTableAsync(sw, naiveBayes);

                await sw.WriteLineAsync();
                await sw.WriteLineAsync("Četnosti predikcí:");
                var predictedCounts = predictResults.GroupBy(t => t)
                .ToDictionary(t => t.Key, t => new
                {
                    Count = t.Count(),
                    Values = t.ToList(),
                });
                foreach (var item in predictedCounts.OrderBy(t => t.Key.ToString()))
                {
                    int count = item.Value.Count;
                    bool preview = count > 10;
                    int take = preview ? 10 : count;
                    await sw.WriteLineAsync($"Shape={item.Key} Count={count}");
                }
                await sw.WriteLineAsync($"Success = {predictSuccess}");
            }
        }
    }
}
