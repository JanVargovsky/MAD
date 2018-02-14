using System.IO;
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

        static async Task Project()
        {
            var csv = new DataMatrixCsvParser();
            var data = await csv.LoadAsync(Filename);
            int dateTimeIndex = data.IndexOf("Date_time");
            data.AddNewAttribute("hour", t => t.Attributes[dateTimeIndex].Substring(t.Attributes[dateTimeIndex].Length - 5, 2));
            data.AddNewAttribute("time", t => t.Attributes[dateTimeIndex].Substring(t.Attributes[dateTimeIndex].Length - 5, 5));
            const string Response = "UFO_shape";
            string[] Predictors = new[] { "length_of_encounter_seconds", "hour" };

            var leaveOneOutCrossValidation = new LeaveOneOutCrossValidation();
            var predictSuccess = leaveOneOutCrossValidation.Validate(data, Response, Predictors, 300);

            var shapeIndex = data.IndexOf(Response);
            var trainingSet = data.Filter((t, i) => !string.IsNullOrEmpty(t.Attributes[shapeIndex]));
            var predictSet = data.Filter((t, i) => string.IsNullOrEmpty(t.Attributes[shapeIndex]));

            var naiveBayes = new NaiveBayes(trainingSet, Response, Predictors);
            //var predicted = naiveBayes.Predict(predictSet[0]);

            var predict = new PredictHelper();
            var predictResults = predict.Predict(naiveBayes, predictSet, Response);

            var report = new TextReport();
            await report.WriteReport("report.txt", trainingSet, trainingSet, predictSet, predictResults, Response, Predictors, naiveBayes, predictSuccess);
        }

        static async Task PresentationData()
        {
            var csv = new DataMatrixCsvParser();
            var data = await csv.LoadAsync("testdata.csv");
            var naiveBayes = new NaiveBayes(data, "class", "day", "season", "wind", "rain");
            var result = naiveBayes.Predict(new DataMatrixRow(-1, new[] { "weekday", "winter", "high", "heavy", "" }));

            var validation = new LeaveOneOutCrossValidation();
            var results = validation.Validate(data, "class", new[] { "day", "season", "wind", "rain" }, data.RowsCount - 1);

            var report = new TextReport();
            using (var sw = new StreamWriter("presentationreport.txt"))
                await report.WriteNaiveBayesProbabilitiesTableAsync(sw, naiveBayes);
        }
    }
}
