using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MAD2.Project
{
    public class SparseMatrix<T> : IEnumerable<T>
    {
        readonly List<T>[] data;

        public int Size => data.Length;

        public SparseMatrix(int size)
        {
            data = new List<T>[size];
            for (int i = 0; i < size; i++)
                data[i] = new List<T>();
        }

        public void Add(int i, T t)
        {
            data[i].Add(t);
        }

        public IEnumerable<T> Get(int i) => data[i];

        public IEnumerator<T> GetEnumerator() => data.SelectMany(t => t).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
