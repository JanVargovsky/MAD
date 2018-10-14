using System;
using System.Text;

namespace MAD3.Lesson3
{
    public class DistanceMatrix : IEquatable<DistanceMatrix>
    {
        internal double[,] Data { get; }

        public double this[int row, int col]
        {
            get => Data[row, col];
            set => Data[row, col] = value;
        }

        public int Size => Data.GetLength(0);

        public DistanceMatrix(int size)
        {
            Data = new double[size, size];
        }

        public (int row, int col) Min()
        {
            int row = 1, col = 0;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (this[i, j] < this[row, col])
                    {
                        row = i;
                        col = j;
                    }
                }
            }

            return (row, col);
        }

        public override string ToString()
        {
            if (Size < 15)
            {
                var sb = new StringBuilder().AppendLine();
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        sb.Append(this[i, j].ToString().PadRight(3));
                    }

                    sb.Append(this[i, i]);
                    if (i != Size - 1)
                        sb.AppendLine(" | ");
                }
                return sb.ToString();
            }
            return base.ToString();
        }

        public bool Equals(DistanceMatrix other)
        {
            if (Size != other.Size) return false;

            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    if (this[i, j] != other[i, j])
                        return false;

            return true;
        }
    }
}
