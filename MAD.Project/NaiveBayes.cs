using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Project
{
    public class NaiveBayes
    {
        // <responseValue, prior>
        internal Dictionary<string, (int Count, double Prior)> PriorProbabilities { get; }
        // <attributeName, <attributeValue, <responseValue, double>>>
        internal Dictionary<string, Dictionary<string, Dictionary<string, double>>> ConditionalProbabilities { get; }
        readonly (int Index, string Name)[] predicators;
        readonly (int Index, string Name) response;
        internal IList<string> Classes { get; }

        public NaiveBayes(DataMatrix trainingSet, string response, params string[] predicators)
        {
            if (predicators.Length == 0)
                predicators = trainingSet.Header.Attributes.Where(t => t != response).ToArray();

            this.response = (trainingSet.IndexOf(response), response);
            this.predicators = predicators.Select(t => (trainingSet.IndexOf(t), t)).ToArray();
            PriorProbabilities = PreprocessPriorProbabilities(trainingSet, trainingSet.IndexOf(response));
            ConditionalProbabilities = PreprocessConditionalProbabilities(trainingSet);
            Classes = trainingSet.Select(t => t[this.response.Index]).Distinct().ToList();
        }

        Dictionary<string, Dictionary<string, Dictionary<string, double>>> PreprocessConditionalProbabilities(DataMatrix trainingSet)
        {
            // <attributeName, ...>
            return predicators.ToDictionary(a => a.Name, a =>
            {
                // <attributeValue, ...>
                return trainingSet.GroupBy(row => row[a.Index])
                    .ToDictionary(t => t.Key, t =>
                    {
                        // <responseValue, ...>
                        var responseValues = t.GroupBy(g => g.Attributes[response.Index])
                            .ToDictionary(v => v.Key, v =>
                            {
                                var c = v.Count() / (double)PriorProbabilities[v.Key].Count;
                                return c;
                            });
                        return responseValues;
                    });
            });
        }

        Dictionary<string, (int, double)> PreprocessPriorProbabilities(DataMatrix trainingSet, int responseIndex)
        {
            return trainingSet.GroupBy(t => t.Attributes[responseIndex])
                .ToDictionary(t => t.Key, t =>
                {
                    var count = t.Count();
                    return (count, count / (double)trainingSet.RowsCount);
                });
        }

        public string Predict(DataMatrixRow row)
        {
            var probabilities = PriorProbabilities.Select(t =>
            {
                var attributeProbabilities = predicators.Select(attribute =>
                {
                    if (!ConditionalProbabilities.TryGetValue(attribute.Name, out var attributeValues))
                        throw new Exception($"unseen attribute: {attribute.Name}");

                    if (!attributeValues.TryGetValue(row.Attributes[attribute.Index], out var responseValues) || 
                        !responseValues.TryGetValue(t.Key, out var p))
                    {
                        // unseen value
                        return 0;
                    }

                    return p;
                }).ToArray();


                var probability = t.Value.Prior;
                foreach (var value in attributeProbabilities)
                    probability *= value;

                return new { Value = t.Key, Probability = probability };
            }).ToArray();

            var predicted = probabilities.MaxBy(t => t.Probability);
            return predicted.Value;
        }
    }
}
