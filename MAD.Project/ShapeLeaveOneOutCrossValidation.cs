using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class ShapeLeaveOneOutCrossValidation
    {
        public float Validate(ICollection<UFORecord> sourceData, int count)
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
                tasks[i] = Task.Run(() => Validate(sourceData, chunks[ii].From, chunks[ii].To));
            }

            Task.WaitAll(tasks);

            int successPredict = tasks.Sum(t => t.Result);
            return successPredict / (float)(sourceData.Count / FractionFactor);
        }

        int Validate(IEnumerable<UFORecord> sourceData, int from, int to)
        {
            var data = sourceData.ToList();
            var toPredict = data[from];
            data.RemoveAt(from);
            int successPredict = 0;

            void CheckPrediction()
            {
                var predictionMethod = new ShapeNaiveBayes(data);
                var predicted = predictionMethod.Predict(toPredict);
                var success = predicted == toPredict.ShapeEnum;
                if (success)
                    successPredict++;
                //ColorConsole.WriteLine(success ? ConsoleColor.Green : ConsoleColor.Red, $"Predicted: {predicted} should be {toPredict.ShapeEnum}");
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
