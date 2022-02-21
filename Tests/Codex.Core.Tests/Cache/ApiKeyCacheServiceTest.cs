using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Exceptions;
using Codex.Models.Security;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Framework.Resources;
using Codex.Tests.Framework;
using CodexGrpc.Security;
using CodexGrpc.Tenants;
using Dapr.Client;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Codex.Core.Tests.Cache;

internal class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        //Project GRPC mapping
        CreateMap<ApiKeyModel, ApiKey>().ReverseMap();
    }
}

public class ApiKeyCacheServiceTest : IClassFixture<Fixture>
{
    private readonly IStringLocalizer<TenantFrameworkResource> _sl;

    private readonly IMapper _mapper;

    public ApiKeyCacheServiceTest()
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
    public void GetCacheKey()
    {
        string apiKeyId = "lkkllkll";
        
        var daprClient = new Mock<DaprClient>();

        var apiKeyCacheServiceLogger = new Mock<ILogger<ApiKeyCacheService>>();
        
        var apiKeyCacheService = new ApiKeyCacheService(daprClient.Object, null, apiKeyCacheServiceLogger.Object, _mapper);

        string cacheKey = apiKeyCacheService.GetCacheKey(new ApiKey(apiKeyId, "", new List<string>()));
        
        Assert.NotNull(cacheKey);
        Assert.Equal($"{CacheConstant.ApiKey_}{apiKeyId}", cacheKey);
    }

    [Fact]
    public async Task SearchApiKey()
    {
        string tenantId = "global";
        string apiKeyId = "lkkllkll";
        var daprClient = new Mock<DaprClient>();

        var apiKeyCacheServiceLogger = new Mock<ILogger<ApiKeyCacheService>>();
        
        var apiKeyServiceClient = new Mock<ApiKeyService.ApiKeyServiceClient>();

        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new ApiKeyModel() {Id = apiKeyId}), 
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
        );

        apiKeyServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Returns(fakeCall);
        
        var apiKeyCacheService = new ApiKeyCacheService(daprClient.Object, null, apiKeyCacheServiceLogger.Object, _mapper);
        apiKeyCacheService.ApiKeyServiceClient = apiKeyServiceClient.Object;
        var apiKey = await apiKeyCacheService.GetApiKeyAsync(apiKeyId, tenantId);

        Assert.NotNull(apiKey);
        Assert.Equal(apiKeyId, apiKey.Id);

        daprClient.Verify(v => v.GetStateAndETagAsync<ApiKey?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
        
        apiKeyServiceClient.Verify(x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SearchApiKeyById_Inside_Cache()
    {
        string tenantId = "global";
        string apiKeyId = "lkkllkll";
        
        var daprClient = new Mock<DaprClient>();
        
        daprClient.Setup(v => v.GetStateAndETagAsync<ApiKey?>(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<(ApiKey?, string?)>((new ApiKey() {Id = apiKeyId}, "")));

        var apiKeyServiceClient = new Mock<ApiKeyService.ApiKeyServiceClient>();
        
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new ApiKeyModel() {Id = apiKeyId}), 
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
        );
        apiKeyServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Returns(fakeCall);

        var apiKeyCacheServiceLogger = new Mock<ILogger<ApiKeyCacheService>>();
        
        var apiKeyCacheService = new ApiKeyCacheService(daprClient.Object, null, apiKeyCacheServiceLogger.Object, _mapper);
        apiKeyCacheService.ApiKeyServiceClient = apiKeyServiceClient.Object;
        var apiKey = await apiKeyCacheService.GetApiKeyAsync(apiKeyId, tenantId);

        Assert.NotNull(apiKey);
        Assert.Equal(apiKeyId, apiKey.Id);

        daprClient.Verify(v => v.GetStateAndETagAsync<ApiKey?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
            
        apiKeyServiceClient.Verify(x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task SearchTenantById_Generic_Exception()
    {
        string tenantId = "global";
        string apiKeyId = "lkkllkll";

        var daprClient = new Mock<DaprClient>();
        
        var apiKeyCacheServiceLogger = new Mock<ILogger<ApiKeyCacheService>>();
        
        var apiKeyServiceClient = new Mock<ApiKeyService.ApiKeyServiceClient>();

        apiKeyServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Throws(new RpcException(new Status(StatusCode.Internal, "invalid tenant")));
        var apiKeyCacheService = new ApiKeyCacheService(daprClient.Object, null, apiKeyCacheServiceLogger.Object, _mapper);
        apiKeyCacheService.ApiKeyServiceClient = apiKeyServiceClient.Object;

        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async () => await apiKeyCacheService.GetApiKeyAsync(apiKeyId, tenantId)
        );

        Assert.NotNull(rpcException);
        Assert.Equal(StatusCode.Internal, rpcException.Status.StatusCode);

        daprClient.Verify(v => v.GetStateAndETagAsync<ApiKey?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        
        apiKeyServiceClient.Verify(
            x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None), Times.Once);
    }
    
    [Fact]
    public async Task SearchTenantById_NotFound_Exception()
    {
        string tenantId = "global";
        string apiKeyId = "lkkllkll";

        var daprClient = new Mock<DaprClient>();
        
        var apiKeyCacheServiceLogger = new Mock<ILogger<ApiKeyCacheService>>();
        
        var apiKeyServiceClient = new Mock<ApiKeyService.ApiKeyServiceClient>();

        apiKeyServiceClient.Setup(x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "invalid tenant")));
        var apiKeyCacheService = new ApiKeyCacheService(daprClient.Object, null, apiKeyCacheServiceLogger.Object, _mapper);
        apiKeyCacheService.ApiKeyServiceClient = apiKeyServiceClient.Object;

        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async () => await apiKeyCacheService.GetApiKeyAsync(apiKeyId, tenantId)
        );

        Assert.NotNull(rpcException);
        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);

        daprClient.Verify(v => v.GetStateAndETagAsync<ApiKey?>(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        
        apiKeyServiceClient.Verify(
            x => x.FindOneAsync(It.IsAny<FindOneApiKeyRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None), Times.Once);
    }
}