using System;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class LeaveOneOutCrossValidation
    {
        public float Validate(DataMatrix sourceData, string response, string[] predicators, int count)
        {
            var tasks = new Task<int>[1];

            int chunkSize = count / tasks.Length;
            var chunks = new(int From, int To)[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
                chunks[i] = (i * chunkSize, (i + 1) * chunkSize);

            for (int i = 0; i < tasks.Length; i++)
            {
                int ii = i;
                tasks[i] = Task.Run(() => Validate(sourceData, response, predicators, chunks[ii].From, chunks[ii].To));
            }

            Task.WaitAll(tasks);

            int successPredict = tasks.Sum(t => t.Result);
            return successPredict / (float)(chunkSize * tasks.Length);
        }

        int Validate(DataMatrix sourceData, string response, string[] predicators, int from, int to)
        {
            var predictIndex = sourceData.IndexOf(response);
            var toPredict = sourceData[from];
            var data = sourceData.Filter((_, i) => i != from);
            int successPredict = 0;

            void CheckPrediction()
            {
                var predictionMethod = new NaiveBayes(data, response, predicators);
                var predicted = predictionMethod.Predict(toPredict);
                var success = predicted == toPredict[predictIndex];
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
