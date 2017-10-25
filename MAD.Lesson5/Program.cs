using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.Lesson5
{
    class Program
    {
        enum Outlook { sunny, overcast, rainy }
        enum Temperature { hot, mild, cool }
        enum Humidity { high, normal }
        enum Windy { TRUE, FALSE }
        enum Play { yes, no }

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

        public IEnumerable<object[]> GenerateAllPossibleCombinations(params object[][] func)
        {
            IEnumerable<object[]> Generate(List<object> currentValues, object[][] f, int skip)
            {
                if (skip == f.Length)
                {
                    yield return currentValues.ToArray();
                    yield break;
                }

                foreach (var item in f[skip])
                {
                    currentValues.Add(item);
                    foreach (var g in Generate(currentValues, f, skip + 1))
                        yield return g;
                    currentValues.RemoveAt(currentValues.Count - 1);
                }
            }

            var l = new List<object>();
            return Generate(l, func, 0);
        }

        static object[] AllValues<T>()
        {
            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type);
            bool isNullable = underlyingType != null;
            if (isNullable)
                type = underlyingType;

            List<object> r = new List<object>();
            foreach (var item in Enum.GetValues(type))
                r.Add(item);
            if (isNullable) r.Add(null);
            return r.ToArray();
        }

        static async Task Main(string[] args)
        {
            void WriteHighlight(string s)
            {
                Console.Write(new string('=', Console.WindowWidth));
                Console.WriteLine(s);
                Console.Write(new string('=', Console.WindowWidth));
            }

            const string GolfFilename = "weather.nominal.csv";
            var p = new Program();
            var data = await p.LoadGolfAsync(GolfFilename);
            var combinations = p.GenerateAllPossibleCombinations(
                AllValues<Outlook?>(),
                AllValues<Temperature?>(),
                AllValues<Humidity?>(),
                AllValues<Windy?>(),
                AllValues<Play>()
                ).ToList();

            WriteHighlight("Vsechny kombinace:");
            foreach (var c in combinations)
                Console.WriteLine(string.Join(',', c));

            var rules = combinations.ToDictionary(t => GolfData.FromStupidArray(t), _ => 0);

            // TODO: check if the instance applies to the rule ... null = WHATEVER DUDE do not use fucking dictionary
            var allRules = rules.Keys.ToList();
            foreach (var instance in data)
                foreach (var rule in allRules)
                {
                    if (GolfData.IsUnderRule(rule, instance))
                        rules[rule]++;
                }

            WriteHighlight("Pouzite pravidla:");
            var appliedRules = rules
                .OrderByDescending(t => t.Value)
                .Where(t => t.Value > 0)
                .ToList();

            appliedRules.ForEach(t => Console.WriteLine($"{string.Join(',', t.Key.AsStupidArray())} = {t.Value}"));
        }
    }
}
