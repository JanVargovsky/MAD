using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAD.Project
{
    class Program
    {
        const string Filename = "ufo_sighting_data.csv";

        static Task Main()
        {
            //return PresentationData();
            return Project();
        }

        static async Task PresentationData()
        {
            var csv = new DataMatrixCsvParser();
            var data = await csv.LoadAsync("testdata.csv");
            var naiveBayes = new NaiveBayes(data, "class", "day", "season", "wind", "rain");
            var result = naiveBayes.Predict(new DataMatrixRow(-1, new[] { "weekday", "winter", "high", "heavy", "" }));

            var validation = new LeaveOneOutCrossValidation();
            var results = validation.Validate(data, "class", new[] { "day", "season", "wind", "rain" }, data.RowsCount - 1);
        }

        static async Task Project()
        {
            var csv = new DataMatrixCsvParser();
            var data = await csv.LoadAsync(Filename);
            int dateTimeIndex = data.IndexOf("Date_time");
            data.AddNewAttribute("hour", t => t.Attributes[dateTimeIndex].Substring(t.Attributes[dateTimeIndex].Length - 5, 2));
            const string Response = "UFO_shape";
            string[] Predictors = new[] { "length_of_encounter_seconds", "hour" };

            var leaveOneOutCrossValidation = new LeaveOneOutCrossValidation();
            var predictSuccess = leaveOneOutCrossValidation.Validate(data, Response, Predictors, 50);

            var shapeIndex = data.IndexOf(Response);
            var trainingSet = data.Filter(t => !string.IsNullOrEmpty(t.Attributes[shapeIndex]));
            var predictSet = data.Filter(t => string.IsNullOrEmpty(t.Attributes[shapeIndex]));

            var naiveBayes = new NaiveBayes(trainingSet, Response, Predictors);
            //var predicted = naiveBayes.Predict(predictSet[0]);

            var predict = new PredictHelper();
            var predictResults = predict.Predict(naiveBayes, predictSet, Response);

            var predictedCounts = predictResults.GroupBy(t => t)
                .ToDictionary(t => t.Key, t => new
                {
                    Count = t.Count(),
                    Values = t.ToList(),
                });

            foreach (var item in predictedCounts.OrderBy(t => t.Key.ToString()))
            {
                int count = item.Value.Count;
                bool preview = count > 10;
                int take = preview ? 10 : count;
                Console.WriteLine($"Shape={item.Key}" +
                    $" Count={count}");
            }
        }

        static async Task Old()
        {
            var csv = new CsvParser();
            var records = await csv.LoadAsync(Filename);
            var trainingSet = records.Where(t => t.ShapeEnum != null).ToList();
            //var trainingSet = records.Where(t => t.ShapeEnum != null && t.ShapeEnum != Shape.light).ToList();
            var predictSet = records.Where(t => t.ShapeEnum == null).ToList();

            var shapeLeaveOneOutCrossValidation = new ShapeLeaveOneOutCrossValidation();
            var predictionSuccess = shapeLeaveOneOutCrossValidation.Validate(trainingSet, 200);
            Console.WriteLine($"Prediction success: {predictionSuccess * 100}%");

            var predict = new PredictHelper();
            var shapeNaiveBayes = new ShapeNaiveBayes(trainingSet);
            //var ids = new[] { 63, 64, 240, 337, 613 };
            //var shape = shapeNaiveBayes.Predict(predictSet.Where(t => ids.Contains(t.Id)));
            //var predicted = predict.PredictShape(shapeNaiveBayes, predictSet.Where(t => ids.Contains(t.Id)));
            var predicted = predict.PredictShape(shapeNaiveBayes, predictSet);

            var predictedCounts = predicted.GroupBy(t => t.ShapeEnum.Value)
                .ToDictionary(t => t.Key, t => new
                {
                    Count = t.Count(),
                    Values = t.ToList(),
                });

            foreach (var item in predictedCounts.OrderBy(t => t.Key.ToString()))
            {
                int count = item.Value.Count;
                bool preview = count > 10;
                int take = preview ? 10 : count;
                Console.WriteLine($"Shape={item.Key}" +
                    $" Count={count}" +
                    $" items = [{string.Join(", ", item.Value.Values.Take(take).Select(tt => tt.Id))}{(preview ? " ..." : "")}]");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Počet vstupních dat: {records.Count}")
                .AppendLine($"Počet vstupních trénovacích dat: {trainingSet.Count}")
                .AppendLine($"Počet vstupních predikovanýc dat: {predictSet.Count}")
                .AppendLine();

            sb.AppendLine("Atributy:")
                .AppendLine(@"Date_time - standardized date and time of sighting")
                .AppendLine("city - location of UFO sighting")
                .AppendLine("state/province - the US state or Canadian province, appears blank for other locations")
                .AppendLine("country - Country of UFO sighting")
                .AppendLine("UFO_shape - a one word description of the \"spacecraft\"")
                .AppendLine("length_of_encounter_seconds - standardized to seconds, length of the observation of the UFO")
                .AppendLine("described_duration _of_encounter - raw description of the length of the encounter(shows uncertainty to previous column)")
                .AppendLine("description - text description of the UFO encounter.Warning column is messy, with some curation it could lend itself to some natural language processing and sentiment analysis.")
                .AppendLine("date_documented - when was the UFO sighting reported")
                .AppendLine("latitude - latitude")
                .AppendLine("longitude - longitude")
                .AppendLine();

            sb.AppendLine("Klasifikace pomoci Naive-Bayes (UFO_shape -> Date_time(hour) + length_of_encounter_seconds + city");
            foreach (var item in predictedCounts.OrderBy(t => t.Key.ToString()))
            {
                int count = item.Value.Count;
                bool preview = count > 10;
                int take = preview ? 10 : count;
                sb.AppendLine($"Shape={item.Key}" +
                    $" Count={count}" +
                    $" items = [{string.Join(", ", item.Value.Values.Take(take).Select(tt => tt.Id))}{(preview ? " ..." : "")}]");
            }

            File.WriteAllText("output.txt", sb.ToString());
        }
    }
}
