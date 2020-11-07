using Codex.Tenants.Api.Services;
using Codex.Tenants.Models;
using Codex.Tests.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Api.Tests
{
    public class TenantPropertiesServiceIT : IClassFixture<Fixture>
    {
        public TenantPropertiesServiceIT()
        {
        }

        [Fact]
        public async Task UpdateProperties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<TenantProperties>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantPropertiesService(tenantRepository.Object);

            var tenant = await tenantService.UpdatePropertiesAsync("Id1", new());

            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant!.Id);
        }

        [Fact]
        public async Task UpdateProperty()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantPropertiesService(tenantRepository.Object);

            var tenant = await tenantService.UpdatePropertyAsync("Id1", "data", new());

            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant!.Id);
        }

        [Fact]
        public async Task DeleteProperty()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantPropertiesService(tenantRepository.Object);

            var tenant = await tenantService.DeletePropertyAsync("Id1", "data");

            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant!.Id);
        }

        [Fact]
        public async Task FindProperty()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id", "name", 
                    new TenantProperties()
                    {
                        { "data", new() { "" } }
                    },
                    null
                ))
            );

            var tenantService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantService.FindPropertyAsync("Id1", "data");

            Assert.NotNull(tenantProperties);
        }


        [Fact]
        public async Task FindProperties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id", "name", new(), null))
            );

            var tenantService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantService.FindPropertiesAsync("Id1");

            Assert.NotNull(tenantProperties);
        }
    }
}
