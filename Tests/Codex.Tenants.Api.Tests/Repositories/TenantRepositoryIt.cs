using Codex.Tests.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Tenants.Api.Repositories.Interfaces;
using Xunit;

namespace Codex.Tenants.Api.Tests.Repositories;

public class TenantRepositoryIt : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;
    private readonly ITenantRepository _tenantRepository;

    public TenantRepositoryIt(DbFixture fixture)
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
            properties: new()
            {
                { "test", new() { "test data" } }
            }
        ));
        

        Assert.NotNull(tenant);
        Assert.Equal("tenant-test", tenant!.Name);

        var tenantProperties = tenant.Properties;
        Assert.NotNull(tenantProperties);
        Assert.Single(tenantProperties);
        Assert.True(tenant.Properties!.ContainsKey("test"));

        //Not updated
        Assert.Equal("TenantId", tenant.Id);
    }

    [Fact]
    public async Task Update_Without_Properties()
    {
        await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

        var tenant = await _tenantRepository.UpdateAsync(new(
            id: "TenantId",
            name: "tenant-test"
        ));

        Assert.NotNull(tenant);
        Assert.Equal("tenant-test", tenant!.Name);
        
        var tenantProperties = tenant.Properties;
        Assert.NotNull(tenantProperties);
        Assert.Single(tenantProperties);
        Assert.True(tenant.Properties!.ContainsKey("data"));

        //Not updated
        Assert.Equal("TenantId", tenant.Id);
    }

    [Fact]
    public async Task Add_Property()
    {
        await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

        Dictionary<string, List<string>> tenantProperties = new()
        {
            ["newProperty"] = new() { "property1", "property2" }
        };

        var tenant = await _tenantRepository.UpdatePropertiesAsync("TenantId", tenantProperties);

        Assert.NotNull(tenant);
        Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["newProperty"]);
    }

    [Fact]
    public async Task Add_Multiple_Property()
    {
        await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

        Dictionary<string, List<string>> tenantProperties = new()
        {
            ["newProperty1"] = new() { "property1", "property2" },
            ["newProperty2"] = new() { "property3", "property4" }
        };

        var tenant = await _tenantRepository.UpdatePropertiesAsync("TenantId", tenantProperties);

        Assert.NotNull(tenant);
        Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["newProperty1"]);
        Assert.Equal(new() { "property3", "property4" }, tenant.Properties["newProperty2"]);
    }

    [Fact]
    public async Task Update_Property()
    {
        await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

        List<string> tenantProperty = new()
        {
            "property1",
            "property2"
        };

        var tenant = await _tenantRepository.UpdatePropertyAsync("TenantId", "data", tenantProperty);

        Assert.NotNull(tenant);
        Assert.Equal(new() { "property1", "property2" }, tenant!.Properties!["data"]);
    }


    [Fact]
    public async Task Update_Properties()
    {
        await _fixture.UseDataSetAsync(locations: @"Resources/tenants.json");

        Dictionary<string, List<string>> tenantProperties = new()
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