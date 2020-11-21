using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Framework.Interfaces;
using Codex.Models.Tenants;
using Codex.Tests.Framework;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Framework.Tests
{
    public class TenantAccessServiceTest : IClassFixture<Fixture>
    {
        public TenantAccessServiceTest()
        {
        }

        [Fact]
        public async Task Get_Null_Tenant()
        {
            var tenantResolutionStrategy = new Mock<ITenantResolutionStrategy>();
            tenantResolutionStrategy.Setup(x => x.GetTenantIdentifierAsync()).Returns(
                Task.FromResult("tenant")
            );

            var tenantStore = new Mock<ITenantStore>();
            tenantStore.Setup(x => x.GetTenantAsync(It.Is<string>(m => m == "tenant"))).Returns(
                Task.FromResult((Tenant?)new Tenant("tenant", "my tenant", null))
            );

            TenantAccessService tenantAccessService = new(tenantResolutionStrategy.Object, tenantStore.Object);

            Tenant? tenant = await tenantAccessService.GetTenantAsync();

            Assert.NotNull(tenant);
            Assert.Equal("tenant", tenant!.Id);
            Assert.Equal("my tenant", tenant!.Name);
        }
    }
}
