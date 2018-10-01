using System.Collections.Generic;
using System.Linq;

namespace MAD3.Lesson2
{
    public class Dataset
    {
        public List<double[]> Data { get; }

        public int NumberOfAttributes => Data[0].Length;

        public Dataset()
        {
            Data = new List<double[]>();
        }

        public double MinOf(int i) => Data.Min(t => t[i]);
        public double MaxOf(int i) => Data.Max(t => t[i]);
    }
}
