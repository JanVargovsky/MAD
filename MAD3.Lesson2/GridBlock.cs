using System;
using System.Diagnostics;

namespace MAD3.Lesson2
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class GridBlock
    {
        readonly int _numberOfChunks;

        public double Min { get; }
        public double Max { get; }
        public double BlockSize { get; }

        public GridBlock(double min, double max, int numberOfChunks)
        {
            Min = min;
            Max = max;
            _numberOfChunks = numberOfChunks;
            BlockSize = Math.Abs(max - min) / numberOfChunks;
        }

        string DebuggerDisplay => $"{Min} - {Max}, {BlockSize}";

        public int GetBlock(double value)
        {
            int result = (int)((value - Min) / BlockSize);
            return Math.Min(result, _numberOfChunks - 1);
        }
    }
}
