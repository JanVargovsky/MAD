using Force.DeepCloner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class Predict
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
    }
}
