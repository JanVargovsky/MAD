using System.Collections.Generic;
using System.Linq;

namespace MAD2.Lesson10
{
    class SparseMatrix<T>
    {
        readonly HashSet<(T From, T To)> edges;

        public SparseMatrix()
        {
            edges = new HashSet<(T From, T To)>();
        }

        public void AddEdge(T a, T b)
        {
            edges.Add((a, b));
            edges.Add((b, a));
        }
    }

    public class Matrix<T>
    {
        readonly T[,] data;
        public int IndexOffset { get; private set; } = 0;

        public T this[int row, int col]
        {
            get => data[row + IndexOffset, col + IndexOffset];
            set => data[row + IndexOffset, col + IndexOffset] = value;
        }

        public T GetRaw(int row, int col) => data[row, col];
        public void SetRaw(int row, int col, T value) => data[row, col] = value;

        public int Size => data.GetLength(0);

        public Matrix(int size)
        {
            data = new T[size, size];
        }

        public Matrix<T> WithIndexOffset(int offset)
        {
            IndexOffset = offset;
            return this;
        }

        public Matrix<T> EmptyCopy<T>() => new Matrix<T>(Size).WithIndexOffset(IndexOffset);
    }
}
