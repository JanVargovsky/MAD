using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson8
{
    class Program
    {
        enum Outlook { sunny, overcast, rainy }
        enum Temperature { hot, mild, cool }
        enum Humidity { high, normal }
        enum Windy { TRUE, FALSE }
        enum Play { yes, no }

        [DebuggerDisplay("{Outlook} {Temperature} {Humidity} {Windy} {Play}")]
        class GolfData
        {
            public Outlook? Outlook { get; set; }
            public Temperature? Temperature { get; set; }
            public Humidity? Humidity { get; set; }
            public Windy? Windy { get; set; }
            public Play Play { get; set; }

            public object[] AsStupidArray() => new object[] { Outlook, Temperature, Humidity, Windy, Play };
            public static GolfData FromStupidArray(object[] a) => new GolfData
            {
                Outlook = (Outlook?)a[0],
                Temperature = (Temperature?)a[1],
                Humidity = (Humidity?)a[2],
                Windy = (Windy?)a[3],
                Play = (Play)a[4],
            };

            public override bool Equals(object obj)
            {
                var data = obj as GolfData;
                return data != null &&
                       Outlook == data.Outlook &&
                       Temperature == data.Temperature &&
                       Humidity == data.Humidity &&
                       Windy == data.Windy &&
                       Play == data.Play;
            }

            public override int GetHashCode()
            {
                var hashCode = -599054754;
                hashCode = hashCode * -1521134295 + EqualityComparer<Outlook?>.Default.GetHashCode(Outlook);
                hashCode = hashCode * -1521134295 + EqualityComparer<Temperature?>.Default.GetHashCode(Temperature);
                hashCode = hashCode * -1521134295 + EqualityComparer<Humidity?>.Default.GetHashCode(Humidity);
                hashCode = hashCode * -1521134295 + EqualityComparer<Windy?>.Default.GetHashCode(Windy);
                hashCode = hashCode * -1521134295 + Play.GetHashCode();
                return hashCode;
            }

            public static bool IsUnderRule(GolfData rule, GolfData instance)
            {
                return (!rule.Outlook.HasValue || (rule.Outlook == instance.Outlook)) &&
                       (!rule.Temperature.HasValue || (rule.Temperature == instance.Temperature)) &&
                       (!rule.Humidity.HasValue || (rule.Humidity == instance.Humidity)) &&
                       (!rule.Windy.HasValue || (rule.Windy == instance.Windy)) &&
                       rule.Play == instance.Play;
            }
        }

        async Task<IEnumerable<GolfData>> LoadGolfAsync(string file)
        {
            var data = new List<GolfData>();
            var lines = await File.ReadAllLinesAsync(file);
            foreach (var line in lines)
            {
                var tokens = line.Split(',');
                data.Add(new GolfData
                {
                    Outlook = Enum.Parse<Outlook>(tokens[0], true),
                    Temperature = Enum.Parse<Temperature>(tokens[1], true),
                    Humidity = Enum.Parse<Humidity>(tokens[2], true),
                    Windy = Enum.Parse<Windy>(tokens[3], true),
                    Play = Enum.Parse<Play>(tokens[4], true),
                });
            }
            return data;
        }


        Play NaiveBayes(ICollection<GolfData> data, GolfData instance)
        {
            int yes = 0, no = 0;
            {
                var yesPlays = data.Where(t => t.Play == Play.yes).ToList();
                int outlookCount = yesPlays.Count(t => t.Outlook == instance.Outlook);
                int temperatureCount = yesPlays.Count(t => t.Temperature == instance.Temperature);
                int humidityCount = yesPlays.Count(t => t.Humidity == instance.Humidity);
                int windyCount = yesPlays.Count(t => t.Windy == instance.Windy);
                yes = outlookCount + temperatureCount + humidityCount + windyCount;
            }
            {
                var noPlays = data.Where(t => t.Play == Play.no).ToList();
                int outlookCount = noPlays.Count(t => t.Outlook == instance.Outlook);
                int temperatureCount = noPlays.Count(t => t.Temperature == instance.Temperature);
                int humidityCount = noPlays.Count(t => t.Humidity == instance.Humidity);
                int windyCount = noPlays.Count(t => t.Windy == instance.Windy);
                no = outlookCount + temperatureCount + humidityCount + windyCount;
            }

            return yes >= no ? Play.yes : Play.no;
        }

        float LeaveOneOutCrossValidation(IList<GolfData> data)
        {
            int bayesSuccess = 0;
            for (int i = 0; i < data.Count; i++)
            {
                var inputData = data.ToList();
                inputData.RemoveAt(i);
                var result = NaiveBayes(inputData, data[i]);
                if (result == data[i].Play)
                    bayesSuccess++;
                else
                    Console.WriteLine($"i={i} ({data[i].Outlook} {data[i].Temperature} {data[i].Humidity} {data[i].Windy} {data[i].Play}) is predicted wrong");
            }
            return bayesSuccess / (float)data.Count;
        }

        static async Task Main(string[] args)
        {
            const string GolfFilename = "weather.nominal.csv";
            var p = new Program();
            var data = (await p.LoadGolfAsync(GolfFilename)).ToList();

            Console.WriteLine($"Accuracy: {p.LeaveOneOutCrossValidation(data)}");
        }
    }
}
