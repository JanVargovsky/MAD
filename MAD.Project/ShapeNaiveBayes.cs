using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Project
{
    public class ShapeNaiveBayes
    {
        readonly IReadOnlyDictionary<Shape, List<UFORecord>> trainingSet;
        readonly int total;

        public ShapeNaiveBayes(ICollection<UFORecord> trainingSet)
        {
            this.total = trainingSet.Count;
            this.trainingSet = trainingSet
                .GroupBy(t => t.ShapeEnum.Value)
                .ToDictionary(t => t.Key, t => t.ToList());
        }

        public Shape Predict(UFORecord toPredict)
        {
            var values = new List<(Shape Shape, float Probability)>();

            float Calculate(UFORecord instance, Shape s)
            {
                var probabilities = new List<float>();

                if (!trainingSet.TryGetValue(s, out var filteredData))
                    return 0;

                void Append(Func<UFORecord, bool> p) => probabilities.Add(filteredData.Count(p) / (float)filteredData.Count);

                const StringComparison StringComparison = StringComparison.OrdinalIgnoreCase;
                // 1
                //Append(u => u.DateTime == instance.DateTime);
                //Append(u => u.DateTime.TimeOfDay == instance.DateTime.TimeOfDay);
                Append(u => u.DateTime.Hour == instance.DateTime.Hour);
                //Append(u => u.City.Equals(instance.City, StringComparison));
                //Append(u => u.StateOrProvince.Equals(instance.StateOrProvince, StringComparison));
                Append(u => u.Country.Equals(instance.Country, StringComparison));
                Append(u => u.Length == instance.Length);
                //Append(u => u.DescribedLength.Equals(instance.DescribedLength, StringComparison));
                //Append(u => u.Description.Equals(instance.Description, StringComparison));
                //Append(u => u.DocumentedAt == instance.DocumentedAt);

                // may I use some radius from the required instance
                //Append(u => u.Latitude == instance.Latitude);
                //Append(u => u.Longitude == instance.Longitude);

                // 2
                //Append(u => u.Length == instance.Length);

                float probability = filteredData.Count / (float)total; // prior probability
                foreach (var p in probabilities)
                    probability *= p;
                return probability;
            }

            foreach (Shape s in Enum.GetValues(typeof(Shape)))
                values.Add((s, Calculate(toPredict, s)));

            var result = values.MaxBy(t => t.Probability);
            return result.Shape;
        }
    }
}
