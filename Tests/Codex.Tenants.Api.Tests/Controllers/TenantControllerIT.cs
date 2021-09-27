using Codex.Core.Models;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Controllers;
using Codex.Tenants.Api.Services;
using Codex.Tests.Framework;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Api.Tests
{
    public class TenantControllerIT : IClassFixture<Fixture>
    {
        public TenantControllerIT()
        {
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            tenantController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await tenantController.FindOne("Id1");

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenant = Assert.IsType<Tenant>(objectResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
            Assert.Null(tenant!.Properties);
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            tenantController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );

            var result = await tenantController.FindOne("Id1");

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenant = Assert.IsType<Tenant>(objectResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
            Assert.NotNull(tenant!.Properties);
            Assert.Single(tenant!.Properties);
            Assert.True(tenant!.Properties!.ContainsKey("test"));
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.FindOne("Id1");

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            tenantController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await tenantController.FindAll();

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenantList = Assert.IsType<List<Tenant>>(objectResult.Value);
            Assert.NotNull(tenantList);
            Assert.Equal(2, tenantList!.Count);

            foreach (var tenant in tenantList)
            {
                Assert.Null(tenant!.Properties);
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            tenantController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );

            var result = await tenantController.FindAll();

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenantList = Assert.IsType<List<Tenant>>(objectResult.Value);
            Assert.NotNull(tenantList);
            Assert.Equal(2, tenantList!.Count);

            foreach (var tenant in tenantList)
            {
                Assert.NotNull(tenant!.Properties);
                Assert.Single(tenant!.Properties);
                Assert.True(tenant!.Properties!.ContainsKey("test"));
            }

            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateTenant()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenantCreator = new Tenant();

            tenantService.Setup(x => x.CreateAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult(new Tenant("Id1", "name"))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.CreateTenant(tenantCreator);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), createdAtActionResult.ActionName);
            var tenant = Assert.IsType<Tenant>(createdAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
            Assert.Equal("name", tenant.Name);
            Assert.Null(tenant.Properties);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTenant_With_Properties()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenantCreator = new Tenant("Id1", "name", new());

            tenantService.Setup(x => x.CreateAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult(new Tenant("Id1", "name", new()))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.CreateTenant(tenantCreator);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), createdAtActionResult.ActionName);
            var tenant = Assert.IsType<Tenant>(createdAtActionResult.Value);
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

            var tenant = new Tenant("Id1", "name", null);

            tenantService.Setup(x => x.UpdateAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.UpdateTenant("Id1", tenant);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.UpdateTenant("Id1", new());

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProperties()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenant = new Tenant("Id1", "name", null);

            tenantPropertiesService.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<TenantProperties>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.UpdateProperties("Id1", new());

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProperties_NotFound()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<TenantProperties>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.UpdateProperties("Id1", new());


            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProperty()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenant = new Tenant("Id1", "name", null);

            tenantPropertiesService.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.UpdateProperty("Id1", "data", new());

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.UpdateProperty("Id1", "data", new());


            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task FindProperties()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.FindPropertiesAsync(It.IsAny<string>())).Returns(
                Task.FromResult((TenantProperties?)new TenantProperties()
                {
                    { "data", new() { "" } },
                    { "data2", new() { "" } }
                })
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.FindProperties("Id1");

            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenantProperties = Assert.IsType<TenantProperties>(okObjectResult.Value);
            Assert.NotNull(tenantProperties);
            Assert.Equal(2, tenantProperties.Count);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProperty()
        {
            var daprClient = CreateMockDaprClientWithTenant();
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.DeleteProperty("Id1", "data");

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
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

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object,
                daprClient.Object
            );

            var result = await tenantController.DeleteProperty("Id1", "data");

            Assert.IsType<NoContentResult>(result.Result);
            daprClient.Verify(x => x.PublishEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TopicData<Tenant>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
