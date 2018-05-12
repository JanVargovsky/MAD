using System.Linq;
using Xunit;

namespace MAD2.Project.Tests
{
    public class NetworkDatasetAnalyserTest
    {
        NetworkDatasetAnalyser analyser = new NetworkDatasetAnalyser();

        [Theory]
        [InlineData(4, 2, 0d)]
        [InlineData(6, 2, 0.16667d)]
        public void ModularityTest(int size, int classesCount, double expectedQ)
        {
            var (sparseMatrix, matrix, nodes) = GetCircularGraph(size);
            var classes = nodes.Select(t => t / classesCount).ToArray();

            var actualQ = analyser.Modularity(sparseMatrix, classes);
            var actualQ2 = analyser.Modularity(matrix, t => t, classes);

            const int Precision = 5;
            Assert.Equal(actualQ, actualQ2, Precision);
            Assert.Equal(expectedQ, actualQ, Precision);
        }

        (SparseMatrix<Edge> sparseMatrix, Matrix<int> matrix, int[] nodes) GetCircularGraph(int nodeCount)
        {
            var nodes = Enumerable.Range(0, nodeCount).ToArray();
            var sparseMatrix = new SparseMatrix<Edge>(nodeCount);
            var matrix = new Matrix<int>(nodeCount);
            const int W = 1;

            void AddEdge(int i, int j)
            {
                sparseMatrix.Add(i, new Edge(i, j, W));
                sparseMatrix.Add(j, new Edge(j, i, W));

                matrix[i, j] = matrix[j, i] = W;
            }

            for (int i = 0; i < nodes.Length - 1; i++)
                AddEdge(nodes[i], nodes[i + 1]);

            AddEdge(nodes.Last(), nodes.First());

            var ds = new DatasetLoader();
            return (sparseMatrix, matrix, nodes);
        }

        [Theory]
        [InlineData(4, 2)]
        public void CommunityDetectionTest(int size, int expectedCommunityCount)
        {
            var (sparseMatrix, matrix, nodes) = GetCircularGraph(size);
            var communities = analyser.CommunityDetection(sparseMatrix, nodes);

            Assert.Equal(expectedCommunityCount, communities.Count);
        }
    }
}
