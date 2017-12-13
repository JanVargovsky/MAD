using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Project
{
    public enum Shape
    {
        unknown,
        cylinder,
        light,
        circle,
        sphere,
        disk,
        fireball,
        oval,
        cigar,
        rectangle,
        chevron,
        triangle,
        formation,
        delta,
        changing,
        egg,
        diamond,
        flash,
        teardrop,
        cone,
        cross,
        pyramid,
        round,
        crescent,
        flare,
        hexagon,
        dome,
        changed,
        other,
    }

    public class UFORecord
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string Country { get; set; }
        public string Shape { get; set; }
        public Shape? ShapeEnum { get; set; }
        public float Length { get; set; } // in sec
        public string DescribedLength { get; set; }
        public string Description { get; set; }
        public DateTime DocumentedAt { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    class Program
    {
        const string Filename = "ufo_sighting_data.csv";

        static async Task Main(string[] args)
        {
            var csv = new CsvParser();
            var records = await csv.LoadAsync(Filename);
            var trainingSet = records.Where(t => t.ShapeEnum != null).ToList();
            //var trainingSet = records.Where(t => t.ShapeEnum != null && t.ShapeEnum != Shape.light).ToList();
            var predictSet = records.Where(t => t.ShapeEnum == null).ToList();

            //var shapeLeaveOneOutCrossValidation = new ShapeLeaveOneOutCrossValidation();
            //var predictionSuccess = shapeLeaveOneOutCrossValidation.Validate(trainingSet, 200);
            //Console.WriteLine($"Prediction success: {predictionSuccess * 100}%");

            var predict = new Predict();
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

            //foreach (var item in predicted)
            //{
            //    Console.WriteLine($"{item.DateTime}: {item.ShapeEnum}");
            //}
        }
    }
}
