using FluentAssertions;
using Xunit;

namespace MAD3.Lesson3.Test
{
    public class HierarchicalAgglomerativeClusteringTest
    {
        [Fact]
        public void SingleLinkageTest()
        {
            var hierarchicalAgglomerativeClustering = new HierarchicalAgglomerativeClustering();
            var distanceMatrix5 = new DistanceMatrix(5)
            {
                [1, 0] = 9,
                [2, 0] = 3,
                [3, 0] = 6,
                [4, 0] = 11,

                [2, 1] = 7,
                [3, 1] = 5,
                [4, 1] = 10,

                [3, 2] = 9,
                [4, 2] = 2,

                [4, 3] = 8,
            };
            var distanceMatrix4 = new DistanceMatrix(4)
            {
                [1, 0] = 3,
                [2, 0] = 7,
                [3, 0] = 8,

                [2, 1] = 9,
                [3, 1] = 6,

                [3, 2] = 5,
            };
            var distanceMatrix3 = new DistanceMatrix(3)
            {
                [1, 0] = 7,
                [2, 0] = 6,

                [2, 1] = 5,
            };
            var distanceMatrix2 = new DistanceMatrix(2)
            {
                [1, 0] = 6,
            };
            var distanceMatrix1 = new DistanceMatrix(1);
            var clusters5 = hierarchicalAgglomerativeClustering.CreateClusters(5);

            var (result4, clusters4) = hierarchicalAgglomerativeClustering.SingleLinkageIteration(distanceMatrix5, clusters5);
            var (result3, clusters3) = hierarchicalAgglomerativeClustering.SingleLinkageIteration(distanceMatrix4, clusters4);
            var (result2, clusters2) = hierarchicalAgglomerativeClustering.SingleLinkageIteration(distanceMatrix3, clusters3);
            var (result1, clusters1) = hierarchicalAgglomerativeClustering.SingleLinkageIteration(distanceMatrix2, clusters2);

            distanceMatrix4.Should().BeEquivalentTo(result4);
            distanceMatrix3.Should().BeEquivalentTo(result3);
            distanceMatrix2.Should().BeEquivalentTo(result2);
            distanceMatrix1.Should().BeEquivalentTo(result1);
        }

        [Fact]
        public void CompleteLinkageIterationTest()
        {
            var hierarchicalAgglomerativeClustering = new HierarchicalAgglomerativeClustering();
            var distanceMatrix5 = new DistanceMatrix(5)
            {
                [1, 0] = 9,
                [2, 0] = 3,
                [3, 0] = 6,
                [4, 0] = 11,

                [2, 1] = 7,
                [3, 1] = 5,
                [4, 1] = 10,

                [3, 2] = 9,
                [4, 2] = 2,

                [4, 3] = 8,
            };
            var distanceMatrix4 = new DistanceMatrix(4)
            {
                [1, 0] = 11,
                [2, 0] = 10,
                [3, 0] = 9,

                [2, 1] = 9,
                [3, 1] = 6,

                [3, 2] = 5,
            };
            var distanceMatrix3 = new DistanceMatrix(3)
            {
                [1, 0] = 10,
                [2, 0] = 9,

                [2, 1] = 11,
            };
            var distanceMatrix2 = new DistanceMatrix(2)
            {
                [1, 0] = 11,
            };
            var distanceMatrix1 = new DistanceMatrix(1);
            var clusters5 = hierarchicalAgglomerativeClustering.CreateClusters(5);

            var (result4, clusters4) = hierarchicalAgglomerativeClustering.CompleteLinkageIteration(distanceMatrix5, clusters5);
            var (result3, clusters3) = hierarchicalAgglomerativeClustering.CompleteLinkageIteration(distanceMatrix4, clusters4);
            var (result2, clusters2) = hierarchicalAgglomerativeClustering.CompleteLinkageIteration(distanceMatrix3, clusters3);
            var (result1, clusters1) = hierarchicalAgglomerativeClustering.CompleteLinkageIteration(distanceMatrix2, clusters2);

            distanceMatrix4.Should().BeEquivalentTo(result4);
            distanceMatrix3.Should().BeEquivalentTo(result3);
            distanceMatrix2.Should().BeEquivalentTo(result2);
            distanceMatrix1.Should().BeEquivalentTo(result1);
        }
    }
}
