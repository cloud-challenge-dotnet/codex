using Codex.Tests.Framework;
using Xunit;


namespace Codex.Core.Tests
{
    public class StringUtilsTest : IClassFixture<Fixture>
    {
        public StringUtilsTest()
        {
        }

        [Fact]
        public void RandomString()
        {
            string result = StringUtils.RandomString(10);

            Assert.NotNull(result);

            Assert.Equal(10, result.Length);
        }

        [Fact]
        public void EmptyRandomString()
        {
            string result = StringUtils.RandomString(0);

            Assert.NotNull(result);

            Assert.Equal(0, result.Length);
        }
    }
}
