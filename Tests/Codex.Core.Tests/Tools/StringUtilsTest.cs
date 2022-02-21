using Codex.Core.Tools;
using Codex.Tests.Framework;
using Xunit;

namespace Codex.Core.Tests.Tools;

public class StringUtilsTest : IClassFixture<Fixture>
{
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