using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Project
{
    public class NaiveBayes
    {
        // <responseValue, prior>
        readonly Dictionary<string, (int Count, double Prior)> priorProbabilities;
        // <attributeName, <attributeValue, <responseValue, double>>>
        readonly Dictionary<string, Dictionary<string, Dictionary<string, double>>> conditionalProbabilities;
        readonly (int Index, string Name)[] predicators;

        public NaiveBayes(DataMatrix trainingSet, string response, params string[] predicators)
        {
            if (predicators.Length == 0)
                predicators = trainingSet.Header.Attributes.Where(t => t != response).ToArray();

            this.predicators = predicators.Select(t => (trainingSet.IndexOf(t), t)).ToArray();
            priorProbabilities = PreprocessPriorProbabilities(trainingSet, trainingSet.IndexOf(response));
            conditionalProbabilities = PreprocessConditionalProbabilities(trainingSet,
                (trainingSet.IndexOf(response), response));
        }

        Dictionary<string, Dictionary<string, Dictionary<string, double>>> PreprocessConditionalProbabilities(DataMatrix trainingSet,
            (int Index, string Name) response)
        {
            // <attributeName, ...>
            return predicators.ToDictionary(a => a.Name, a =>
            {
                //var tmpA = trainingSet.GroupBy(row => row[response.Index]).ToDictionary(t => t.Key, t => t.ToList());
                //var tmpB = trainingSet.GroupBy(row => row[a.Index]).ToDictionary(t => t.Key, t => t.ToList());

                // <attributeValue, ...>
                return trainingSet.GroupBy(row => row[a.Index])
                    .ToDictionary(t => t.Key, t =>
                    {
                        // <responseValue, ...>
                        var responseValues = t.GroupBy(g => g.Attributes[response.Index])
                            .ToDictionary(v => v.Key, v =>
                            {
                                var c = v.Count() / (double)priorProbabilities[v.Key].Count;
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
            var probabilities = priorProbabilities.Select(t =>
            {
                var attributeProbabilities = predicators.Select(attribute =>
                {
                    if (!conditionalProbabilities.TryGetValue(attribute.Name, out var attributeValues))
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

        //Dictionary<string, (double Prior, double[] Conditionals)> Preprocess(DataMatrix trainingSet,
        //    (int Index, string Name) response,
        //    (int Index, string Name)[] predicatorIndexes)
        //{
        //    Dictionary<string, double[]> PreprocessAttributeValues(List<DataMatrixRow> rows, string responseValue, int attributeIndex)
        //    {
        //        return rows.GroupBy(row => row.Attributes[attributeIndex])
        //            .ToDictionary(g => g.Key, g =>
        //            {

        //                var a = rows.Count(r => r.Attributes[response.Index] == responseValue) / (double)rows.Count;
        //                return new double[0];
        //            });
        //    }

        //    var res = trainingSet.GroupBy(t => t.Attributes[response.Index]) // response
        //        .ToDictionary(t => t.Key, g =>
        //        {
        //            var records = g.ToList();
        //            // is g.Key value of response?
        //            var result = predicatorIndexes.ToDictionary(t => t.Name, t => PreprocessAttributeValues(records, g.Key, t.Index));
        //            return (records.Count / (double)trainingSet.RowsCount, result);
        //        });

        //    return null;
        //}
    }
}
