using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public class DotNetCoreTests
    {

        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, Add(2, 2));
        }

        private static int Add(int x, int y)
        {
            return x + y;
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        public void MyFirstTheory(int value)
        {
            Assert.True(IsOdd(value));
        }

        private static bool IsOdd(int value)
        {
            return value % 2 == 1;
        }

        [Fact]
        public void KeyValueNullTest() {
            Dictionary<int, string> dict = new Dictionary<int, string> {{1, "hello"}, {2, "good-bye"} };
            string x = dict.SingleOrDefault(a => a.Key == 0).Value;
            Assert.Null(x);
        }
    }
}
