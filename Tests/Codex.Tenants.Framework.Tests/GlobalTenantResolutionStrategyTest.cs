using Codex.Tenants.Framework.Implementations;
using Codex.Tests.Framework;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Framework.Tests
{
    public class GlobalTenantResolutionStrategyTest : IClassFixture<Fixture>
    {
        public GlobalTenantResolutionStrategyTest()
        {
        }

        [Fact]
        public async Task GetTenantIdentifier()
        {
            GlobalTenantResolutionStrategy tenantStrategy = new();

            string? tenantId = await tenantStrategy.GetTenantIdentifierAsync();

            Assert.NotNull(tenantId);
            Assert.Equal("global", tenantId);
        }
    }
}
