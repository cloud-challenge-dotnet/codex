using Codex.Tenants.Api.Services;
using Codex.Models.Tenants;
using Codex.Tests.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Codex.Tenants.Api.Repositories.Interfaces;

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

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenant = await tenantPropertiesService.UpdatePropertiesAsync("Id1", new());

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

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenant = await tenantPropertiesService.UpdatePropertyAsync("Id1", "data", new());

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

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenant = await tenantPropertiesService.DeletePropertyAsync("Id1", "data");

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
                    }
                ))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantPropertiesService.FindPropertyAsync("Id1", "data");

            Assert.NotNull(tenantProperties);
        }

        [Fact]
        public async Task FindProperty_Null_Property()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id", "name",
                    null
                ))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantPropertiesService.FindPropertyAsync("Id1", "data");

            Assert.Null(tenantProperties);
        }


        [Fact]
        public async Task FindProperty_Null_Tenant()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantPropertiesService.FindPropertyAsync("Id1", "data");

            Assert.Null(tenantProperties);
        }

        [Fact]
        public async Task FindProperties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id", "name", new()))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantPropertiesService.FindPropertiesAsync("Id1");

            Assert.NotNull(tenantProperties);
        }

        [Fact]
        public async Task FindProperties_Null_Properties()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id", "name", null))
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantPropertiesService.FindPropertiesAsync("Id1");

            Assert.Null(tenantProperties);
        }

        [Fact]
        public async Task FindProperties_Null_Tenant()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantPropertiesService = new TenantPropertiesService(tenantRepository.Object);

            var tenantProperties = await tenantPropertiesService.FindPropertiesAsync("Id1");

            Assert.Null(tenantProperties);
        }
    }
}
