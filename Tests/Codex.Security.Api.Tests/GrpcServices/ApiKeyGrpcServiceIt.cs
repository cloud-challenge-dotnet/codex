using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Models.Security;
using Codex.Security.Api.GrpcServices;
using Codex.Security.Api.Services.Interfaces;
using Codex.Tests.Framework;
using Dapr.Client;
using Moq;
using Xunit;
using AutoMapper;
using Codex.Core.Tools.AutoMapper;
using Codex.Security.Api.MappingProfiles;
using CodexGrpc.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Codex.Security.Api.Tests.GrpcServices;

public class ApiKeyGrpcServiceIt : IClassFixture<Fixture>
{
    private readonly IMapper _mapper;
        
    public ApiKeyGrpcServiceIt()
    {
        //auto mapper configuration
        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile<CoreMappingProfile>();
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<Codex.Core.MappingProfiles.GrpcMappingProfile>();
            cfg.AddProfile<GrpcMappingProfile>();
        });
        _mapper = mockMapper.CreateMapper();
    }

    [Fact]
    public async Task FindOne()
    {
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((ApiKey?)new ApiKey() { Id = "Id1" })
        );

        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );

        var authorizeAttributes = apiKeyGrpcService.GetType().GetMethod(nameof(ApiKeyGrpcService.FindOne))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var result = await apiKeyGrpcService.FindOne(new(){Id = "Id1"}, Fixture.CreateServerCallContext());

        apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

        Assert.NotNull(result);
        Assert.Equal("Id1", result.Id);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task FindOne_NotFound()
    {
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((ApiKey?)null)
        );
            
        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await apiKeyGrpcService.FindOne(new(){Id = "Id1"}, Fixture.CreateServerCallContext()));

        apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
            
        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);
    }

    [Fact]
    public async Task FindAll()
    {
        ApiKeyCriteriaModel apiKeyCriteria = new();
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>())).Returns(
            Task.FromResult(new List<ApiKey>(){
                new() { Id = "Id1" },
                new() { Id = "Id2" }
            })
        );

        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );

        var authorizeAttributes = apiKeyGrpcService.GetType().GetMethod(nameof(ApiKeyGrpcService.FindAll))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var result = await apiKeyGrpcService.FindAll(
            new () { Criteria = apiKeyCriteria },
            Fixture.CreateServerCallContext()
        );

        apiKeyService.Verify(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>()), Times.Once);

        Assert.NotNull(result);
        Assert.NotNull(result.ApiKeys);
        Assert.Equal(2, result.ApiKeys.Count);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task CreateApiKey()
    {
        ApiKeyModel apiKeyCreator = new() { Name = "ApiKey 1" };
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.CreateAsync(It.IsAny<ApiKey>())).Returns(
            Task.FromResult(new ApiKey() { Id = "Id1" })
        );

        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var authorizeAttributes = apiKeyGrpcService.GetType().GetMethod(nameof(ApiKeyGrpcService.Create))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var result = await apiKeyGrpcService.Create(apiKeyCreator, serverCallContext);

        apiKeyService.Verify(x => x.CreateAsync(It.IsAny<ApiKey>()), Times.Once);

        var apiKey = Assert.IsType<ApiKeyModel>(result);
        Assert.NotNull(apiKey);
        Assert.Equal("Id1", apiKey.Id);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task UpdateApiKey()
    {
        ApiKeyModel apiKey = new() { Id = "Id1", Name = "ApiKey 1" };
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.UpdateAsync(It.IsAny<ApiKey>())).Returns(
            Task.FromResult((ApiKey?)new ApiKey() { Id = "Id1", Name = "ApiKey 1" })
        );
            
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );

        var authorizeAttributes = apiKeyGrpcService.GetType().GetMethod(nameof(ApiKeyGrpcService.Update))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var result = await apiKeyGrpcService.Update(apiKey, serverCallContext);

        apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
        apiKeyService.Verify(x => x.UpdateAsync(It.IsAny<ApiKey>()), Times.Once);

        var apiKeyResult = Assert.IsType<ApiKeyModel>(result);
        Assert.NotNull(apiKeyResult);
        Assert.Equal("Id1", apiKeyResult.Id);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task UpdateApiKey_Not_Found_ApiKey()
    {
        ApiKeyModel apiKey = new() { Id = "Id1", Name = "ApiKey 1" };
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.UpdateAsync(It.IsAny<ApiKey>())).Returns(
            Task.FromResult((ApiKey?)null)
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );

        var authorizeAttributes = apiKeyGrpcService.GetType().GetMethod(nameof(ApiKeyGrpcService.Update))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await apiKeyGrpcService.Update(apiKey, serverCallContext));

        apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
        apiKeyService.Verify(x => x.UpdateAsync(It.IsAny<ApiKey>()), Times.Once);

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);
            
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task DeleteApiKey()
    {
        string apiKeyId = "Id1";
        var apiKeyService = new Mock<IApiKeyService>();
        var daprClient = new Mock<DaprClient>();

        apiKeyService.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(
            Task.CompletedTask
        );

        var apiKeyGrpcService = new ApiKeyGrpcService(
            _mapper,
            daprClient.Object,
            apiKeyService.Object
        );
            
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var authorizeAttributes = apiKeyGrpcService.GetType().GetMethod(nameof(ApiKeyGrpcService.Delete))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var result = await apiKeyGrpcService.Delete(new(){Id = apiKeyId}, serverCallContext);
        Assert.IsType<Empty>(result);

        apiKeyService.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Once);
            
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }
}