using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Models;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Tenants.Api.GrpcServices;
using Codex.Tenants.Api.MappingProfiles;
using Codex.Tenants.Api.Services.Interfaces;
using Codex.Tests.Framework;
using CodexGrpc.Tenants;
using Dapr.Client;
using Grpc.Core;
using Moq;
using Xunit;

namespace Codex.Tenants.Api.Tests.GrpcServices;

public class TenantControllerIt : IClassFixture<Fixture>
{
    private readonly IMapper _mapper;
    
    public TenantControllerIt()
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

    private static Mock<DaprClient> CreateMockDaprClientWithTenant(Tenant? tenant = null)
    {
        var daprClient = new Mock<DaprClient>();
        daprClient.Setup(x => x.GetStateAndETagAsync<Tenant?>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ConsistencyMode?>(), It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()
        )).Returns(Task.FromResult<(Tenant?, string)>((tenant, "")));

        return daprClient;
    }

    [Fact]
    public async Task FindOne()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((Tenant?)new Tenant("Id1", "name",
                Properties: new()
                {
                    { "test", new() { "test data" } }
                }
            ))
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var result = await tenantGrpcService.FindOne(new(){Id = "Id1"}, serverCallContext);

        var tenant = Assert.IsType<TenantModel>(result);
        Assert.NotNull(tenant);
        Assert.Equal("Id1", tenant.Id);
        Assert.Empty(tenant.Properties);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task FindOne_Role_TENANT_MANAGER()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((Tenant?)new Tenant("Id1", "name",
                Properties: new()
                {
                    { "test", new() { "test data" } }
                }
            ))
        );

        
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TenantManager }
            )
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var result = await tenantGrpcService.FindOne(new(){Id = "Id1"}, serverCallContext);

        var tenant = Assert.IsType<TenantModel>(result);
        Assert.NotNull(tenant);
        Assert.Equal("Id1", tenant.Id);
        Assert.NotNull(tenant.Properties);
        Assert.Single(tenant.Properties);
        Assert.True(tenant.Properties!.ContainsKey("test"));
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FindOne_NotFound()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((Tenant?)null)
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async ()=> await tenantGrpcService.FindOne(new(){Id = "Id1"}, Fixture.CreateServerCallContext())
        );

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);

        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FindAll()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantService.Setup(x => x.FindAllAsync()).Returns(
            Task.FromResult(new List<Tenant>(){
                new Tenant("Id1", "name",
                    Properties: new()
                    {
                        { "test", new() { "test data" } }
                    }
                ),
                new Tenant("Id2", "name",
                    Properties: new()
                    {
                        { "test", new() { "test data" } }
                    }
                )
            })
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var result = await tenantGrpcService.FindAll(new(), serverCallContext);

        var tenantList = result.Tenants;
        Assert.NotNull(tenantList);
        Assert.Equal(2, tenantList!.Count);

        foreach (var tenant in tenantList)
        {
            Assert.Empty(tenant!.Properties);
        }

        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FindAll_Role_TENANT_MANAGER()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantService.Setup(x => x.FindAllAsync()).Returns(
            Task.FromResult(new List<Tenant>(){
                new Tenant("Id1", "name",
                    Properties: new()
                    {
                        { "test", new() { "test data" } }
                    }
                ),
                new Tenant("Id2", "name",
                    Properties: new()
                    {
                        { "test", new() { "test data" } }
                    }
                )
            })
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TenantManager }
            )
        );
        
        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );
        
        var result = await tenantGrpcService.FindAll(new(), serverCallContext);

        var tenantList = result.Tenants;
        Assert.NotNull(tenantList);
        Assert.Equal(2, tenantList!.Count);

        foreach (var tenant in tenantList)
        {
            Assert.NotNull(tenant!.Properties);
            Assert.Single(tenant.Properties);
            Assert.True(tenant.Properties!.ContainsKey("test"));
        }

        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTenant()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        var tenantCreator = new TenantModel();

        tenantService.Setup(x => x.CreateAsync(It.IsAny<Tenant>())).Returns(
            Task.FromResult(new Tenant("Id1", "name"))
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenant = await tenantGrpcService.Create(tenantCreator, Fixture.CreateServerCallContext());

        Assert.NotNull(tenant);
        Assert.Equal("Id1", tenant.Id);
        Assert.Equal("name", tenant.Name);
        Assert.Empty(tenant.Properties);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTenant_With_Properties()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        var tenantCreator = new TenantModel()
        {
            Id = "Id1",
            Name = "name"
        };

        tenantService.Setup(x => x.CreateAsync(It.IsAny<Tenant>())).Returns(
            Task.FromResult(new Tenant("Id1", "name", new()))
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenant = await tenantGrpcService.Create(tenantCreator, Fixture.CreateServerCallContext());

        Assert.NotNull(tenant);
        Assert.Equal("Id1", tenant.Id);
        Assert.Equal("name", tenant.Name);
        Assert.NotNull(tenant.Properties);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTenant()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        var tenant = new TenantModel(){
            Id = "Id1",
            Name = "name"
        };

        tenantService.Setup(x => x.UpdateAsync(It.IsAny<Tenant>())).Returns(
            Task.FromResult((Tenant?)new Tenant("Id1", "name"))
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenantResult = await tenantGrpcService.Update(tenant, Fixture.CreateServerCallContext());

        Assert.NotNull(tenantResult);
        Assert.Equal("Id1", tenantResult.Id);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTenant_NotFound()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantService.Setup(x => x.UpdateAsync(It.IsAny<Tenant>())).Returns(
            Task.FromResult((Tenant?)null)
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async ()=> await tenantGrpcService.Update(new(){Id="Id1"}, Fixture.CreateServerCallContext())
        );

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);

        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProperties()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>())).Returns(
            Task.FromResult((Tenant?)new Tenant("Id1", "name"))
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenantResult = await tenantGrpcService.UpdateProperties(new(), Fixture.CreateServerCallContext());

        Assert.NotNull(tenantResult);
        Assert.Equal("Id1", tenantResult.Id);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProperties_NotFound()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>())).Returns(
            Task.FromResult((Tenant?)null)
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async ()=> await tenantGrpcService.UpdateProperties(new(){TenantId = "Id1"}, Fixture.CreateServerCallContext())
        );

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);

        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProperty()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(
            Task.FromResult((Tenant?)new Tenant("Id1", "name"))
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenantResult = await tenantGrpcService.UpdateProperty(new()
        {
            TenantId = "Id1",
            PropertyKey = "data"
        }, Fixture.CreateServerCallContext());
        
        Assert.NotNull(tenantResult);
        Assert.Equal("Id1", tenantResult.Id);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProperty_NotFound()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(
            Task.FromResult((Tenant?)null)
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async ()=> await tenantGrpcService.UpdateProperty(new(){
                TenantId = "Id1",
                PropertyKey = "data"
            }, Fixture.CreateServerCallContext())
        );

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);

        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FindProperties()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.FindPropertiesAsync(It.IsAny<string>())).Returns(
            Task.FromResult((Dictionary<string, List<string>>?)new Dictionary<string, List<string>>()
            {
                { "data", new() { "" } },
                { "data2", new() { "" } }
            })
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenantPropertiesResponse = await tenantGrpcService.FindProperties(new(){ TenantId = "Id1"}, Fixture.CreateServerCallContext());

        Assert.NotNull(tenantPropertiesResponse);
        Assert.Equal(2, tenantPropertiesResponse.Properties.Count);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProperty()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
            Task.FromResult((Tenant?)new Tenant("Id1", "name"))
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );

        var tenantResult = await tenantGrpcService.DeleteProperty(new()
        {
            TenantId = "Id1",
            PropertyKey = "data"
        }, Fixture.CreateServerCallContext());

        Assert.NotNull(tenantResult);
        Assert.Equal("Id1", tenantResult.Id);
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProperty_No_Content()
    {
        var daprClient = CreateMockDaprClientWithTenant();
        var tenantService = new Mock<ITenantService>();
        var tenantPropertiesService = new Mock<ITenantPropertiesService>();

        tenantPropertiesService.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
            Task.FromResult((Tenant?)null)
        );

        var tenantGrpcService = new TenantGrpcService(
            _mapper,
            daprClient.Object,
            tenantService.Object,
            tenantPropertiesService.Object
        );
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(
            async ()=> await tenantGrpcService.DeleteProperty(new(){
                TenantId = "Id1",
                PropertyKey = "data"
            }, Fixture.CreateServerCallContext())
        );

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);
        
        daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}