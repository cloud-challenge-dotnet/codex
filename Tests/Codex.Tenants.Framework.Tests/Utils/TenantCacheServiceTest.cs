using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Codex.Tenants.Framework.Resources;
using Codex.Tests.Framework;
using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Tools.AutoMapper;
using Codex.Tenants.Framework.Implementations;
using CodexGrpc.Tenants;
using Grpc.Core.Testing;
using Xunit;

namespace Codex.Tenants.Framework.Tests.Utils;

internal class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        //Project GRPC mapping
        CreateMap<TenantModel, Tenant>().ReverseMap();
    }
}

public class TenantCacheServiceTest : IClassFixture<Fixture>
{
    private readonly IStringLocalizer<TenantFrameworkResource> _sl;

    private readonly IMapper _mapper;

    public TenantCacheServiceTest()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        _sl = new StringLocalizer<TenantFrameworkResource>(factory);
        
        
        //auto mapper configuration
        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile<CoreMappingProfile>();
            cfg.AddProfile<Codex.Core.MappingProfiles.GrpcMappingProfile>();
            cfg.AddProfile<GrpcMappingProfile>();
        });
        _mapper = mockMapper.CreateMapper();
    }

    [Fact]
    public async Task SearchTenantById()
    {
        string tenantId = "global";
        
        var daprClient = new Mock<DaprClient>();

        var tenantCacheServiceLogger = new Mock<ILogger<TenantCacheService>>();
        
        var tenantServiceClient = new Mock<TenantService.TenantServiceClient>();

        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new TenantModel() {Id = "global"}), 
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
        );

        tenantServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None))
            .Returns(fakeCall);
        
        var tenantCacheService = new TenantCacheService(tenantCacheServiceLogger.Object, _mapper, daprClient.Object, _sl);
        tenantCacheService.TenantServiceClient = tenantServiceClient.Object;
        var tenant = await tenantCacheService.GetTenantAsync(tenantId);

        Assert.NotNull(tenant);
        Assert.Equal("global", tenant.Id);

        daprClient.Verify(v => v.GetStateAndETagAsync<Tenant?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
        
        tenantServiceClient.Verify(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SearchTenantById_Inside_Cache()
    {
        string tenantId = "global";
        
        var daprClient = new Mock<DaprClient>();
        
        daprClient.Setup(v => v.GetStateAndETagAsync<Tenant?>(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(Tenant?, string?)>((new Tenant() {Id = "global"}, "")));

        var tenantServiceClient = new Mock<TenantService.TenantServiceClient>();
        
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new TenantModel() {Id = "global"}), 
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
        );
        tenantServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None))
            .Returns(fakeCall);

        var tenantCacheServiceLogger = new Mock<ILogger<TenantCacheService>>();
        
        var tenantCacheService = new TenantCacheService(tenantCacheServiceLogger.Object, _mapper, daprClient.Object, _sl);
        tenantCacheService.TenantServiceClient = tenantServiceClient.Object;
        var tenant = await tenantCacheService.GetTenantAsync(tenantId);

        Assert.NotNull(tenant);
        Assert.Equal("global", tenant.Id);

        daprClient.Verify(v => v.GetStateAndETagAsync<Tenant?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
            
        tenantServiceClient.Verify(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task SearchTenantById_Generic_Exception()
    {
        string tenantId = "global";

        var daprClient = new Mock<DaprClient>();
        
        var tenantCacheServiceLogger = new Mock<ILogger<TenantCacheService>>();
        
        var tenantServiceClient = new Mock<TenantService.TenantServiceClient>();

        tenantServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None))
            .Throws(new RpcException(new Status(StatusCode.Internal, "invalid tenant")));
        
        var tenantCacheService = new TenantCacheService(tenantCacheServiceLogger.Object, _mapper, daprClient.Object, _sl);
        tenantCacheService.TenantServiceClient = tenantServiceClient.Object;

        var technicalException = await Assert.ThrowsAsync<TechnicalException>(
            async () => await tenantCacheService.GetTenantAsync(tenantId)
        );

        Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);

        daprClient.Verify(v => v.GetStateAndETagAsync<Tenant?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        
        tenantServiceClient.Verify(
            x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SearchTenantById_Aborted_RpcException()
    {
        string tenantId = "global";

        var daprClient = new Mock<DaprClient>();

        var tenantCacheServiceLogger = new Mock<ILogger<TenantCacheService>>();
        
        var tenantServiceClient = new Mock<TenantService.TenantServiceClient>();
        
        tenantServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None))
            .Throws(new RpcException(new Status(StatusCode.Aborted, "")));
        
        var tenantCacheService = new TenantCacheService(tenantCacheServiceLogger.Object, _mapper, daprClient.Object, _sl);
        tenantCacheService.TenantServiceClient = tenantServiceClient.Object;

        var technicalException = await Assert.ThrowsAsync<TechnicalException>(
            async () => await tenantCacheService.GetTenantAsync(tenantId)
        );

        Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);

        daprClient.Verify(v => v.GetStateAndETagAsync<Tenant?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        tenantServiceClient.Verify(
            x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SearchTenantById_Notfound_RpcException()
    {
        string tenantId = "global";

        var daprClient = new Mock<DaprClient>();
        
        var tenantCacheServiceLogger = new Mock<ILogger<TenantCacheService>>();
        
        var tenantServiceClient = new Mock<TenantService.TenantServiceClient>();
        
        tenantServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "")));

        var tenantCacheService = new TenantCacheService(tenantCacheServiceLogger.Object, _mapper, daprClient.Object, _sl);
        tenantCacheService.TenantServiceClient = tenantServiceClient.Object;
        
        var invalidTenantIdException = await Assert.ThrowsAsync<InvalidTenantIdException>(
            async () => await tenantCacheService.GetTenantAsync(tenantId)
        );

        Assert.Equal("TENANT_NOT_FOUND", invalidTenantIdException.Code);

        daprClient.Verify(v => v.GetStateAndETagAsync<Tenant?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        
        tenantServiceClient.Verify(
            x => x.FindOneAsync(It.IsAny<FindOneTenantRequest>(), null, null, CancellationToken.None), Times.Once);
    }
}