using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class ShapeLeaveOneOutCrossValidation
    {
        public float Validate(ShapeNaiveBayes predictionMethod, ICollection<UFORecord> sourceData) =>
            Validate(predictionMethod, sourceData, sourceData.Count);

        public float Validate(ShapeNaiveBayes predictionMethod, ICollection<UFORecord> sourceData, int count)
        {
            var tasks = new Task<int>[Environment.ProcessorCount];

            const int FractionFactor = 500;
            int chunkSize = sourceData.Count / FractionFactor / tasks.Length;

            var chunks = new(int From, int To)[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
                chunks[i] = (i * chunkSize, (i + 1) * chunkSize);

            for (int i = 0; i < tasks.Length; i++)
            {
                int ii = i;
                tasks[i] = Task.Run(() => Validate(predictionMethod, sourceData, chunks[ii].From, chunks[ii].To));
            }

            Task.WaitAll(tasks);

            int successPredict = tasks.Sum(t => t.Result);
            return successPredict / (float)(sourceData.Count / FractionFactor);

            //var data = sourceData.ToList();
            //var toPredict = data[0];
            //data.RemoveAt(0);
            //int successPredict = 0;

            //void CheckPrediction()
            //{
            //    var predicted = predictionMethod.Predict(data, toPredict);
            //    if (predicted == toPredict.ShapeEnum)
            //        successPredict++;
            //}

            //for (int i = 0; i < count; i++)
            //{
            //    CheckPrediction();

            //    var tmp = data[i];
            //    data[i] = toPredict;
            //    toPredict = tmp;
            //}

            //CheckPrediction();

            //return successPredict / (float)count;
        }

        int Validate(ShapeNaiveBayes predictionMethod, IEnumerable<UFORecord> sourceData, int from, int to)
        {
            var data = sourceData.ToList();
            var toPredict = data[from];
            data.RemoveAt(from);
            int successPredict = 0;

            void CheckPrediction()
            {
                var predicted = predictionMethod.Predict(data, toPredict);
                if (predicted == toPredict.ShapeEnum)
                    successPredict++;
            }

            for (int i = from; i < to; i++)
            {
                CheckPrediction();

                var tmp = data[i];
                data[i] = toPredict;
                toPredict = tmp;
            }

            CheckPrediction();

            return successPredict;
        }
    }
}
