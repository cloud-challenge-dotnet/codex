using AutoMapper;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Tenants.Api.Repositories.Models;
using Codex.Tenants.Api.Services;
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
        private readonly IMapper _mapper;

        public TenantServiceIT()
        {
            //auto mapper configuration
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = null;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mockMapper.CreateMapper();
        }

        [Fact]
        public async Task FindAll()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            tenantRepository.Setup(x => x.FindAllAsync()).Returns(
                Task.FromResult(new List<TenantRow>()
                {
                    new("Id1", "Tenant 1", null),
                    new("Id2", "Tenant 2", null)
                })
            );

            var tenantService = new TenantService(tenantRepository.Object, _mapper);

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
                Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1", null))
            );


            var tenantService = new TenantService(tenantRepository.Object, _mapper);

            var tenant = await tenantService.FindOneAsync(tenandId);

            Assert.NotNull(tenant);
            Assert.Equal(tenandId, tenant!.Id);
        }

        [Fact]
        public async Task Create()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenantCreator = new Tenant("Id1", "Tenant 1");

            tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
               Task.FromResult(false)
           );

            tenantRepository.Setup(x => x.InsertAsync(It.IsAny<TenantRow>())).Returns(
                Task.FromResult(new TenantRow("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantService(tenantRepository.Object, _mapper);

            var tenant = await tenantService.CreateAsync(tenantCreator);

            Assert.NotNull(tenant);
            Assert.Equal(tenantCreator.Id, tenant.Id);
        }

        [Fact]
        public async Task Create_Existing_Tenant()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenantCreator = new Tenant("Id1", "Tenant 1");

            tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
                Task.FromResult(true)
            );

            var tenantService = new TenantService(tenantRepository.Object, _mapper);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => tenantService.CreateAsync(tenantCreator));

            Assert.IsType<IllegalArgumentException>(exception);
            Assert.Contains("TENANT_EXISTS", exception.Code);
        }

        [Fact]
        public async Task Create_Null_Tenant_Id()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenantCreator = new Tenant(null, "Tenant 1");

            tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
                Task.FromResult(true)
            );

            var tenantService = new TenantService(tenantRepository.Object, _mapper);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => tenantService.CreateAsync(tenantCreator));

            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task Update()
        {
            var tenantRepository = new Mock<ITenantRepository>();

            var tenant = new Tenant("Id1", "Tenant 1", null);

            tenantRepository.Setup(x => x.UpdateAsync(It.IsAny<TenantRow>())).Returns(
                Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1", null))
            );

            var tenantService = new TenantService(tenantRepository.Object, _mapper);

            var tenantUpdated = await tenantService.UpdateAsync(tenant);

            Assert.NotNull(tenantUpdated);
            Assert.Equal(tenant.Id, tenantUpdated!.Id);
        }
    }
}
