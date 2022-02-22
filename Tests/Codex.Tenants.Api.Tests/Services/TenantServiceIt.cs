using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Api.MappingProfiles;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Tenants.Api.Repositories.Models;
using Codex.Tenants.Api.Resources;
using Codex.Tenants.Api.Services.Implementations;
using Codex.Tests.Framework;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Codex.Tenants.Api.Tests.Services;

public class TenantServiceIt : IClassFixture<Fixture>
{
    private readonly IMapper _mapper;

    private readonly IStringLocalizer<TenantResource> _sl;

    public TenantServiceIt()
    {
        //auto mapper configuration
        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(new CoreMappingProfile());
            cfg.AddProfile(new MappingProfile());
        });
        _mapper = mockMapper.CreateMapper();

        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        _sl = new StringLocalizer<TenantResource>(factory);
    }

    [Fact]
    public async Task FindAll()
    {
        var tenantRepository = new Mock<ITenantRepository>();

        tenantRepository.Setup(x => x.FindAllAsync()).Returns(
            Task.FromResult(new List<TenantRow>()
            {
                new("Id1", "Tenant 1"),
                new("Id2", "Tenant 2")
            })
        );

        var tenantService = new TenantService(tenantRepository.Object, _mapper, _sl);

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

        string tenantId = "Id1";

        tenantRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1"))
        );


        var tenantService = new TenantService(tenantRepository.Object, _mapper, _sl);

        var tenant = await tenantService.FindOneAsync(tenantId);

        Assert.NotNull(tenant);
        Assert.Equal(tenantId, tenant!.Id);
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
            Task.FromResult(new TenantRow("Id1", "Tenant 1"))
        );

        var tenantService = new TenantService(tenantRepository.Object, _mapper, _sl);

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

        var tenantService = new TenantService(tenantRepository.Object, _mapper, _sl);

        var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => tenantService.CreateAsync(tenantCreator));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Code);
        Assert.Contains("TENANT_EXISTS", exception.Code!);
    }

    [Fact]
    public async Task Create_Tenant_With_Empty_Id()
    {
        var tenantRepository = new Mock<ITenantRepository>();

        var tenantCreator = new Tenant("", "Tenant 1");

        tenantRepository.Setup(x => x.ExistsByIdAsync(It.IsAny<string>())).Returns(
            Task.FromResult(true)
        );

        var tenantService = new TenantService(tenantRepository.Object, _mapper, _sl);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => tenantService.CreateAsync(tenantCreator));

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public async Task Update()
    {
        var tenantRepository = new Mock<ITenantRepository>();

        var tenant = new Tenant("Id1", "Tenant 1");

        tenantRepository.Setup(x => x.UpdateAsync(It.IsAny<TenantRow>())).Returns(
            Task.FromResult((TenantRow?)new TenantRow("Id1", "Tenant 1"))
        );

        var tenantService = new TenantService(tenantRepository.Object, _mapper, _sl);

        var tenantUpdated = await tenantService.UpdateAsync(tenant);

        Assert.NotNull(tenantUpdated);
        Assert.Equal(tenant.Id, tenantUpdated!.Id);
    }
}