using Codex.Core.Exceptions;
using Codex.Tenants.Api.Services;
using Codex.Tenants.Models;
using Codex.Tests.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Api.Tests
{
    public class TenantServiceIT : IClassFixture<Fixture>
    {
        public TenantServiceIT()
        {
        }

        [Fact]
        public async Task FindAll() 
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindAllAsync()).Returns(
                Task.FromResult(new List<Tenant>()
                {
                    new("Id1", "Tenant 1", null),
                    new("Id2", "Tenant 2", null)
                })
            );


            var tenantService = new TenantService(tenantRepository.Object);

            var tenantList = await tenantService.FindAllAsync();

            Assert.NotNull(tenantList);
            Assert.Equal(2, tenantList.Count);

            Assert.Equal("Id1", tenantList[0].Id);
            Assert.Equal("Id2", tenantList[1].Id);
        }

        [Fact]
        public async Task FindOne()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            string tenandId = "Id1";

            tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult(new Tenant("Id1", "Tenant 1", null))
            );


            var tenantService = new TenantService(tenantRepository.Object);

            var tenant = await tenantService.FindOneAsync(tenandId);

            Assert.NotNull(tenant);
            Assert.Equal(tenandId, tenant.Id);
        }

        [Fact]
        public async Task Create()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenantCreator = new TenantCreator("Id1", "Tenant 1");

            tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
               Task.FromResult(false)
           );

            tenantRepository.Setup(x => x.InsertAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult(new Tenant("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantService(tenantRepository.Object);

            var tenant = await tenantService.CreateAsync(tenantCreator);

            Assert.NotNull(tenant);
            Assert.Equal(tenantCreator.Id, tenant.Id);
        }

        [Fact]
        public async Task Create_Existing_Tenant()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenantCreator = new TenantCreator("Id1", "Tenant 1");

            tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
                Task.FromResult(true)
            );

            var tenantService = new TenantService(tenantRepository.Object);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => tenantService.CreateAsync(tenantCreator));

            Assert.IsType<IllegalArgumentException>(exception);
            Assert.Contains("TENANT_EXISTS", exception.Code);
        }

        [Fact]
        public async Task Create_Null_Tenant_Id()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenantCreator = new TenantCreator(null, "Tenant 1");

            tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
                Task.FromResult(true)
            );

            var tenantService = new TenantService(tenantRepository.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => tenantService.CreateAsync(tenantCreator));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task Update()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenant = new Tenant("Id1", "Tenant 1", null);

            tenantRepository.Setup(x => x.UpdateAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult(new Tenant("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantService(tenantRepository.Object);

            var tenantUpdated = await tenantService.UpdateAsync(tenant);

            Assert.NotNull(tenantUpdated);
            Assert.Equal(tenant.Id, tenantUpdated.Id);
        }
    }
}
