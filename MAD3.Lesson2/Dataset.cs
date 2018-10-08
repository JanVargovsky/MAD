using System.Collections.Generic;
using System.Linq;

namespace MAD3.Lesson2
{
    public interface IDataset
    {
        double this[int row, int column] { get; }
        int NumberOfAttributes { get; }
        int Count { get; }

        double MinOf(int column);
        double MaxOf(int column);
    }

    public class RowDataset : IDataset
    {
        public double this[int row, int column] => Rows[row][column];
        public List<double[]> Rows { get; }
        public int NumberOfAttributes => Rows[0].Length;
        public int Count => Rows.Count;

        public RowDataset()
        {
            Rows = new List<double[]>();
        }

        public void Add(double[] values)
        {
            Rows.Add(values);
        }

        public double MinOf(int column) => Rows.Min(t => t[column]);
        public double MaxOf(int column) => Rows.Max(t => t[column]);
    }

    public class ColumnDataset : IDataset
    {
        public double this[int row, int column] => Columns[column][row];
        public List<double>[] Columns { get; }
        public int NumberOfAttributes => Columns.Length;
        public int Count => Columns[0].Count;

        public ColumnDataset(int columns)
        {
            Columns = new List<double>[columns];
            for (int i = 0; i < columns; i++)
                Columns[i] = new List<double>();
        }

        public ColumnDataset(List<double>[] columns) => Columns = columns;

        public void Add(double[] values)
        {
            for (int i = 0; i < values.Length; i++)
                Columns[i].Add(values[i]);
        }

        public double MinOf(int column) => Columns[column].Min();
        public double MaxOf(int column) => Columns[column].Max();
    }

    public static class RowDatasetExtensions
    {
        public static ColumnDataset AsColumnDataset(this RowDataset dataset)
        {
            var columnDataset = new ColumnDataset(dataset.NumberOfAttributes);

            foreach (var row in dataset.Rows)
                columnDataset.Add(row);

            return columnDataset;
        }
    }
}
