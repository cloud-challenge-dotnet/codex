using Codex.Core.Implementations;
using Codex.Tests.Framework;
using Xunit;

namespace Codex.Core.Tests.Implementations;

public class PasswordHasherTest : IClassFixture<Fixture>
{
    [Fact]
    public void GenerateMultipleHash()
    {
        string password = "azertyuiop";

        PasswordHasher passwordHasher = new();
        var salt = passwordHasher.GenerateRandowSalt();

        string passwordHash = passwordHasher.GenerateHash(password, salt);
        Assert.NotNull(passwordHash);

        for (int i = 0; i < 10; i++)
        {
            string passwordHash2 = passwordHasher.GenerateHash(password, salt);
            Assert.NotNull(passwordHash2);
            Assert.Equal(passwordHash2, passwordHash);
        }
    }
}