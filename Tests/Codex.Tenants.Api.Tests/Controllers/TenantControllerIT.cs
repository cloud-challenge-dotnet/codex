using Codex.Core.Exceptions;
using Codex.Tenants.Api.Controllers;
using Codex.Tenants.Api.Services;
using Codex.Tenants.Models;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Api.Tests
{
    public class TenantControllerIT : IClassFixture<Fixture>
    {
        public TenantControllerIT()
        {
        }

        [Fact]
        public async Task FindOne()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.FindOne("Id1");

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenant = Assert.IsType<Tenant>(objectResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
        }

        [Fact]
        public async Task FindOne_NotFound()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.FindOne("Id1");

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task FindAll()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantService.Setup(x => x.FindAllAsync()).Returns(
                Task.FromResult(new List<Tenant>(){
                    new Tenant("Id1", "name", null),
                    new Tenant("Id2", "name", null)
                })
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.FindAll();

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenantList = Assert.IsType<List<Tenant>>(objectResult.Value);
            Assert.NotNull(tenantList);
            Assert.Equal(2, tenantList!.Count);
        }

        [Fact]
        public async Task CreateTenant()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenantCreator = new TenantCreator();

            tenantService.Setup(x => x.CreateAsync(It.IsAny<TenantCreator>())).Returns(
                Task.FromResult(new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.CreateTenant(tenantCreator);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), createdAtActionResult.ActionName);
            var tenant = Assert.IsType<Tenant>(createdAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
        }

        [Fact]
        public async Task UpdateTenant()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenant = new Tenant("Id1", "name", null);

            tenantService.Setup(x => x.UpdateAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.UpdateTenant("Id1", tenant);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
        }

        [Fact]
        public async Task UpdateTenant_NotFound()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantService.Setup(x => x.UpdateAsync(It.IsAny<Tenant>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.UpdateTenant("Id1", new());

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task UpdateProperties()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenant = new Tenant("Id1", "name", null);

            tenantPropertiesService.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<TenantProperties>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.UpdateProperties("Id1", new());

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
        }

        [Fact]
        public async Task UpdateProperties_NotFound()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.UpdatePropertiesAsync(It.IsAny<string>(), It.IsAny<TenantProperties>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.UpdateProperties("Id1", new());


            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task UpdateProperty()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            var tenant = new Tenant("Id1", "name", null);

            tenantPropertiesService.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>() , It.IsAny<List<string>>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.UpdateProperty("Id1", "data", new());

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenant);
            Assert.Equal("Id1", tenant.Id);
        }

        [Fact]
        public async Task UpdateProperty_NotFound()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.UpdatePropertyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.UpdateProperty("Id1", "data", new());


            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task FindProperties()
        {
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
                tenantPropertiesService.Object
            );

            var result = await tenantController.FindProperties("Id1");

            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var tenantProperties = Assert.IsType<TenantProperties>(okObjectResult.Value);
            Assert.NotNull(tenantProperties);
            Assert.Equal(2, tenantProperties.Count);
        }

        [Fact]
        public async Task DeleteProperty()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant("Id1", "name", null))
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.DeleteProperty("Id1", "data");

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(tenantController.FindOne), acceptedAtActionResult.ActionName);
            var tenantResult = Assert.IsType<Tenant>(acceptedAtActionResult.Value);
            Assert.NotNull(tenantResult);
            Assert.Equal("Id1", tenantResult.Id);
        }

        [Fact]
        public async Task DeleteProperty_No_Content()
        {
            var tenantService = new Mock<ITenantService>();
            var tenantPropertiesService = new Mock<ITenantPropertiesService>();

            tenantPropertiesService.Setup(x => x.DeletePropertyAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)null)
            );

            var tenantController = new TenantController(
                tenantService.Object,
                tenantPropertiesService.Object
            );

            var result = await tenantController.DeleteProperty("Id1", "data");

            Assert.IsType<NoContentResult>(result.Result);
        }
    }
}
