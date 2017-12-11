using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Project
{
    public class ShapeNaiveBayes
    {
        public Shape Predict(ICollection<UFORecord> data, UFORecord instance)
        {
            var values = new List<(Shape Shape, int Probability)>();

            int Calculate(ICollection<UFORecord> d, Shape s)
            {
                List<int> probabilities = new List<int>();
                var filteredData = d.Where(t => t.ShapeEnum == s).ToList();

                void Append(Func<UFORecord, bool> p) => probabilities.Add(filteredData.Count(p));

                const StringComparison StringComparison = StringComparison.OrdinalIgnoreCase;
                Append(u => u.DateTime.TimeOfDay == instance.DateTime.TimeOfDay);
                Append(u => u.City.Equals(instance.City, StringComparison));
                Append(u => u.StateOrProvince.Equals(instance.StateOrProvince, StringComparison));
                Append(u => u.Country.Equals(instance.Country, StringComparison));
                Append(u => u.Length == instance.Length);
                Append(u => u.Shape.Equals(instance.Shape, StringComparison));
                Append(u => u.DescribedLength.Equals(instance.DescribedLength, StringComparison));
                Append(u => u.Description.Equals(instance.Description, StringComparison));
                Append(u => u.Latitude == instance.Latitude);
                Append(u => u.Longitude == instance.Longitude);

                return probabilities.Sum();
            }

            foreach (Shape s in Enum.GetValues(typeof(Shape)))
                values.Add((s, Calculate(data, s)));

            return values.MaxBy(t => t.Probability).Shape;
        }
    }
}
