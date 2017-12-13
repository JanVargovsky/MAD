using MoreLinq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class PredictHelper
    {
        public List<string> Predict(NaiveBayes naiveBayes, DataMatrix predictSet, string response)
        {
            int index = predictSet.IndexOf(response);
#if !DEBUG
            var result = Enumerable.Range(0, predictSet.RowsCount).Select(t => (string)null).ToList();
            Parallel.ForEach(predictSet, (t, _, i) => result[(int)i] = t[index] = naiveBayes.Predict(t));
            return result;
#else
            return predictSet.Select(t => t[index] = naiveBayes.Predict(t)).ToList();
#endif
        }
    }
}
