using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MAD.Project
{
    public class DataMatrix : IEnumerable<DataMatrixRow>
    {
        public DataMatrixRow Header { get; }
        List<DataMatrixRow> Rows { get; }

        public DataMatrixRow this[int row]
        {
            get => Rows[row];
            set => Rows[row] = value;
        }

        public int RowsCount => Rows.Count;

        public DataMatrix(string[] headerAttributes)
        {
            Header = new DataMatrixRow(-1, headerAttributes);
            Rows = new List<DataMatrixRow>();
        }

        DataMatrix(DataMatrixRow header, List<DataMatrixRow> rows)
        {
            Header = header;
            Rows = rows;
        }

        public void Add(DataMatrixRow row) => Rows.Add(row);

        public DataMatrix Filter(Func<DataMatrixRow, bool> func) => new DataMatrix(Header, Rows.Where(func).ToList());
        public DataMatrix Filter(Func<DataMatrixRow, int, bool> func) => new DataMatrix(Header, Rows.Where(func).ToList());

        public int IndexOf(string columnName) => Header.Attributes.IndexOf(columnName);

        public IEnumerator<DataMatrixRow> GetEnumerator() => Rows.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddNewAttribute(string name, Func<DataMatrixRow, string> func)
        {
            Header.Attributes.Add(name);
            Rows.ForEach(t => t.Attributes.Add(func(t)));
        }
    }
}
