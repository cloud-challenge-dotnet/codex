using Codex.Tenants.Models;
using Codex.Tests.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Api.Tests
{
    public class TenantRepositoryIT : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;
        private readonly ITenantRepository _tenantRepository;

        public TenantRepositoryIT(DbFixture fixture)
        {
            _fixture = fixture;
            _tenantRepository = _fixture.Services.GetService<ITenantRepository>()!;
        }

        [Fact]
        public async Task FindAll()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            var tenantList = await _tenantRepository.FindAllAsync();

            Assert.NotNull(tenantList);
            Assert.Equal(2, tenantList.Count);

            //Not updated
            Assert.Equal("TenantId", tenantList[0].Id);
            Assert.Equal("TenantId2", tenantList[1].Id);
        }


        [Fact]
        public async Task Update()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            var tenant = await _tenantRepository.UpdateAsync(new(
                id: "TenantId",
                name: "tenant-test",
                key: "123131546"
            ));

            Assert.NotNull(tenant);
            Assert.Equal("tenant-test", tenant.Name);

            //Not updated
            Assert.Equal("TenantId", tenant.Id);
            Assert.Equal("TenantKey", tenant.Key);
        }

        [Fact]
        public async Task Update_TenantKey()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            var tenant = await _tenantRepository.UpdateKeyAsync("TenantId", "newKey");

            Assert.NotNull(tenant);
            Assert.Equal("newKey", tenant.Key);

            //Not updated
            Assert.Equal("TenantId", tenant.Id);
            Assert.Equal("TenantName", tenant.Name);
            Assert.Equal(new() { "0", "1", "2", "3" }, tenant.Properties!["data"]);
        }

        [Fact]
        public async Task Add_Property()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            TenantProperties tenantProperties = new()
            {
                ["newProperty"] = new() {"property1", "property2"}
            };

            var tenant = await _tenantRepository.UpdatePropertiesAsync("TenantId", tenantProperties);

            Assert.NotNull(tenant);
            Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["newProperty"]);
        }

        [Fact]
        public async Task Add_Multiple_Property()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            TenantProperties tenantProperties = new()
            {
                ["newProperty1"] = new() { "property1", "property2" },
                ["newProperty2"] = new() { "property3", "property4" }
            };

            var tenant = await _tenantRepository.UpdatePropertiesAsync("TenantId", tenantProperties);

            Assert.NotNull(tenant);
            Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["newProperty1"]);
            Assert.Equal(new() { "property3", "property4" }, tenant!.Properties!["newProperty2"]);
        }

        [Fact]
        public async Task Update_Property()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            List<string> tenantProperty = new()
            {
                "property1", "property2"
            };

            var tenant = await _tenantRepository.UpdatePropertyAsync("TenantId", "data", tenantProperty);

            Assert.NotNull(tenant);
            Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["data"]);
        }


        [Fact]
        public async Task Update_Properties()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            TenantProperties tenantProperties = new()
            {
                ["data"] = new() { "property1", "property2" },
            };

            var tenant = await _tenantRepository.UpdatePropertiesAsync("TenantId", tenantProperties);

            Assert.NotNull(tenant);
            Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["data"]);
        }

        [Fact]
        public async Task Delete_Property()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

            var tenant = await _tenantRepository.DeletePropertyAsync("TenantId", "data");

            Assert.NotNull(tenant);
            Assert.False(tenant!.Properties!.ContainsKey("data"));
        }
    }
}
