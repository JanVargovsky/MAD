using Force.DeepCloner;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class PredictHelper
    {
        public IList<UFORecord> PredictShape(ShapeNaiveBayes shapeNaiveBayes, IEnumerable<UFORecord> predictSet)
        {
            var result = predictSet.Select(t => t.DeepClone()).ToList();
#if !DEBUG
            Parallel.ForEach(result, t =>
            {
                t.ShapeEnum = shapeNaiveBayes.Predict(t);
            });
#else
            result.ForEach(t => t.ShapeEnum = shapeNaiveBayes.Predict(t));
#endif
            return result;
        }

        public List<string> Predict(NaiveBayes naiveBayes, DataMatrix predictSet, string response)
        {
            int index = predictSet.IndexOf(response);
#if DEBUG
            var result = Enumerable.Range(0, predictSet.RowsCount).Select(t => (string)null).ToList();
            Parallel.ForEach(predictSet, t => t[index] = naiveBayes.Predict(t));
            return result;
#else
            return predictSet.Select(t => naiveBayes.Predict(t)).ToList();
#endif
        }
    }
}
