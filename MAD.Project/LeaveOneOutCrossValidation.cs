using System;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public class LeaveOneOutCrossValidation
    {

        public float Validate(DataMatrix sourceData, string response, string[] predicators, int count)
        {
            var tasks = new Task<int>[Environment.ProcessorCount];

            int chunkSize = count / tasks.Length;
            var chunks = new(int From, int To)[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
                chunks[i] = (i * chunkSize, (i + 1) * chunkSize);

            var progress = new Progress<(int Validated, int TotalSize, int WorkerId)>(t => Console.WriteLine($"Validated {t.Validated}/{t.TotalSize} (Id={t.WorkerId})"));

            for (int i = 0; i < tasks.Length; i++)
            {
                int ii = i;
                tasks[i] = Task.Run(() => Validate(sourceData, response, predicators, chunks[ii].From, chunks[ii].To, progress));
            }

            Task.WaitAll(tasks);

            int successPredict = tasks.Sum(t => t.Result);
            return successPredict / (float)(chunkSize * tasks.Length);
        }

        int Validate(DataMatrix sourceData, string response, string[] predicators, int from, int to, IProgress<(int Validated, int TotalSize, int WorkerId)> progress)
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
                //ColorConsole.WriteLine(success ? ConsoleColor.Green : ConsoleColor.Red, $"Predicted: {predicted} should be {toPredict[predictIndex]}");
            }

            int total = to - from;
            int reportFraction = total / 10;
            int count = 0;

            for (int i = from; i < to; i++)
            {
                CheckPrediction();

                var tmp = data[i];
                data[i] = toPredict;
                toPredict = tmp;

                count++;
                if (count % reportFraction == 0)
                    progress.Report((count, total, Task.CurrentId.Value));
            }

            CheckPrediction();

            return successPredict;
        }
    }
}
