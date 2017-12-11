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

        async Task<List<UFORecord>> LoadAsync(string filename)
        {
            var result = new List<UFORecord>();

            using (var sr = new StreamReader(filename))
            {
                // skip header
                await sr.ReadLineAsync();

                var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";

                UFORecord ParseLine(string l)
                {
                    var tokens = l.Split(',');

                    DateTime ParseDateTime(string token)
                    {
                        var parts = token.Split(' ');
                        var date = parts[0].Split('/').Select(int.Parse).ToArray();
                        var time = parts[1].Split(':').Select(int.Parse).ToArray();

                        if (time[0] == 24)
                        {
                            time[0] = 23;
                            time[1] = 59;
                        }

                        return new DateTime(date[2], date[0], date[1], time[0], time[1], 0, DateTimeKind.Utc);
                    }

                    DateTime ParseDateDocumented(string token)
                    {
                        var date = token.Split('/').Select(int.Parse).ToArray();
                        return new DateTime(date[2], date[0], date[1], 0, 0, 0, DateTimeKind.Utc);
                    }

                    var datetime = ParseDateTime(tokens[0]);
                    var documented = ParseDateDocumented(tokens[8]);
                    var length = float.Parse(tokens[5], NumberStyles.Any, ci);
                    var latitude = float.Parse(tokens[9], NumberStyles.Any, ci);
                    var longitude = float.Parse(tokens[10], NumberStyles.Any, ci);
                    Enum.TryParse<Shape>(tokens[4], out var shape);

                    return new UFORecord
                    {
                        Id = result.Count,
                        DateTime = datetime,
                        City = tokens[1],
                        StateOrProvince = tokens[2],
                        Country = tokens[3],
                        Shape = tokens[4],
                        ShapeEnum = shape,
                        Length = length,
                        DescribedLength = tokens[6],
                        Description = tokens[7],
                        DocumentedAt = documented,
                        Latitude = latitude,
                        Longitude = longitude,
                    };
                }

                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    try
                    {
                        var record = ParseLine(line);
                        result.Add(record);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{result.Count} is invalid");
                    }
                }
            }

            return result;
        }

        static async Task Main(string[] args)
        {
            var p = new Program();
            var records = await p.LoadAsync(Filename);

            var shapeNaiveBayes = new ShapeNaiveBayes();
            var shapeLeaveOneOutCrossValidation = new ShapeLeaveOneOutCrossValidation();
            var predictionSuccess = shapeLeaveOneOutCrossValidation.Validate(shapeNaiveBayes, records, 20);
            Console.WriteLine($"Prediction success: {predictionSuccess * 100}%");
        }
    }
}
