using System.Collections;
using System.Collections.Generic;
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
        [InlineData(6, 3)]
        public void CommunityDetectionTest(int size, int expectedCommunityCount)
        {
            var (_, matrix, nodes) = GetCircularGraph(size);
            var communities = analyser.CommunityDetection(matrix, nodes);

            Assert.Equal(expectedCommunityCount, communities.Size);
        }

        // https://arxiv.org/pdf/0803.0476.pdf Page 3, Figure 1
        public class FastUnfoldingOfCommunitiesInLargeNetworksData : IEnumerable<object[]>
        {
            (Matrix<int>, int[]) InitialPhase()
            {
                var nodes = Enumerable.Range(0, 16).ToArray();
                var m = new Matrix<int>(16);
                void AddUndirected(int i, int j) => m[i, j] = m[j, i] = 1;
                AddUndirected(0, 2);
                AddUndirected(0, 3);
                AddUndirected(0, 4);
                AddUndirected(0, 5);

                AddUndirected(1, 2);
                AddUndirected(1, 4);
                AddUndirected(1, 7);

                AddUndirected(2, 4);
                AddUndirected(2, 5);
                AddUndirected(2, 6);

                AddUndirected(3, 7);

                AddUndirected(4, 10);

                AddUndirected(5, 7);
                AddUndirected(5, 11);

                AddUndirected(6, 7);
                AddUndirected(6, 11);

                AddUndirected(8, 9);
                AddUndirected(8, 10);
                AddUndirected(8, 11);
                AddUndirected(8, 14);
                AddUndirected(8, 15);

                AddUndirected(9, 12);
                AddUndirected(9, 14);

                AddUndirected(10, 11);
                AddUndirected(10, 12);
                AddUndirected(10, 13);
                AddUndirected(10, 14);

                AddUndirected(11, 13);

                return (m, nodes);
            }

            (Matrix<int>, int[]) AfterFirstPass()
            {
                const int Green = 0;
                const int Blue = 1;
                const int Red = 2;
                const int White = 3;
                var nodes = new[]
                {
                    Green, Blue, Red, White,
                };

                // after 1st pass
                var m = new Matrix<int>(4)
                {
                    [Green, Green] = 14,
                    [Blue, Blue] = 4,
                    [Red, Red] = 16,
                    [White, White] = 2,
                };
                void AddUndirectedWeighted(int i, int j, int w) => m[i, j] = m[j, i] = w;
                AddUndirectedWeighted(Green, Blue, 4);
                AddUndirectedWeighted(Green, Red, 1);
                AddUndirectedWeighted(Green, White, 1);
                AddUndirectedWeighted(Blue, White, 1);
                AddUndirectedWeighted(Red, White, 3);

                return (m, nodes);
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                var finalCommunityMatrix = new Matrix<int>(2)
                {
                    [0, 0] = 26,
                    [0, 1] = 3,
                    [1, 0] = 3,
                    [1, 1] = 24
                };

                var (initPhaseMatrix, initPhaseNodes) = InitialPhase();
                yield return new object[] { initPhaseMatrix, initPhaseNodes, finalCommunityMatrix };

                var (afterFirstPassMatrix, afterFirstPassNodes) = AfterFirstPass();
                yield return new object[] { afterFirstPassMatrix, afterFirstPassNodes, finalCommunityMatrix };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(FastUnfoldingOfCommunitiesInLargeNetworksData))]
        public void CommunityDetectionWhitePaperExampleTest(Matrix<int> matrix, int[] nodes, Matrix<int> expected)
        {
            var actualCommunityMatrix = analyser.CommunityDetection(matrix, nodes);

            Assert.Equal(expected, actualCommunityMatrix);
        }
    }
}
