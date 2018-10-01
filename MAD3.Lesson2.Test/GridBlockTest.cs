using Xunit;

namespace MAD3.Lesson2.Test
{
    public class GridBlockTest
    {
        [Fact]
        public void GetBlockTest()
        {
            var block = new GridBlock(0, 10, 10);

            Assert.Equal(0, block.GetBlock(0f));
            Assert.Equal(0, block.GetBlock(0.5f));
            Assert.Equal(0, block.GetBlock(0.9999f));

            Assert.Equal(1, block.GetBlock(1f));
            Assert.Equal(1, block.GetBlock(1.5f));
            Assert.Equal(1, block.GetBlock(1.9999f));

            Assert.Equal(9, block.GetBlock(9f));
            Assert.Equal(9, block.GetBlock(9.5f));
            Assert.Equal(9, block.GetBlock(9.9999f));
            Assert.Equal(9, block.GetBlock(10f));
        }
    }
}
