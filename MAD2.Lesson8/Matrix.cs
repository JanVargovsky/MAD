using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MAD2.Lesson8
{
    public class Matrix<T> : IEnumerable<T>
    {
        readonly T[,] data;
        int indexOffset = 0;

        public T this[int row, int col]
        {
            get => data[row + indexOffset, col + indexOffset];
            set => data[row + indexOffset, col + indexOffset] = value;
        }

        public int Size => data.GetLength(0);

        public Matrix(int size)
        {
            data = new T[size, size];
        }

        public Matrix<T> WithIndexOffset(int offset)
        {
            indexOffset = offset;
            return this;
        }

        public IEnumerator<T> GetEnumerator() => data.Cast<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    }
}
