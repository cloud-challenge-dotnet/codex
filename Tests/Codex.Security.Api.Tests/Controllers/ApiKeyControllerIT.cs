using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Codex.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Codex.Security.Api.Services.Interfaces;
using Codex.Models.Security;
using Codex.Security.Api.Controllers;
using Codex.Core.Security;
using Dapr.Client;

namespace Codex.Security.Api.Tests
{
    public class ApiKeyControllerIT : IClassFixture<Fixture>
    {
        private readonly Fixture _fixture;

        public ApiKeyControllerIT(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task FindOne()
        {
            var apiKeyService = new Mock<IApiKeyService>();
            var daprClient = new Mock<DaprClient>();

            apiKeyService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((ApiKey?)new ApiKey() { Id = "Id1" })
            );

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            var authorizeAttributes = apiKeyController.GetType().GetMethod(nameof(ApiKeyController.FindOne))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await apiKeyController.FindOne("Id1");

            apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiKey = Assert.IsType<ApiKey>(objectResult.Value);
            Assert.NotNull(apiKey);
            Assert.Equal("Id1", apiKey.Id);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task FindOne_NotFound()
        {
            var apiKeyService = new Mock<IApiKeyService>();
            var daprClient = new Mock<DaprClient>();

            apiKeyService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((ApiKey?)null)
            );

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            var result = await apiKeyController.FindOne("Id1");

            apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task FindAll()
        {
            ApiKeyCriteria apiKeyCriteria = new();
            var apiKeyService = new Mock<IApiKeyService>();
            var daprClient = new Mock<DaprClient>();

            apiKeyService.Setup(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>())).Returns(
                Task.FromResult(new List<ApiKey>(){
                    new() { Id = "Id1" },
                    new() { Id = "Id2" }
                })
            );

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            var authorizeAttributes = apiKeyController.GetType().GetMethod(nameof(ApiKeyController.FindAll))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await apiKeyController.FindAll(apiKeyCriteria);

            apiKeyService.Verify(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiKeyList = Assert.IsType<List<ApiKey>>(objectResult.Value);
            Assert.NotNull(apiKeyList);
            Assert.Equal(2, apiKeyList!.Count);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task CreateApiKey()
        {
            ApiKey apiKeyCreator = new() { Name = "ApiKey 1" };
            var apiKeyService = new Mock<IApiKeyService>();
            var daprClient = new Mock<DaprClient>();

            apiKeyService.Setup(x => x.CreateAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult(new ApiKey() { Id = "Id1" })
            );

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            apiKeyController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var authorizeAttributes = apiKeyController.GetType().GetMethod(nameof(ApiKeyController.CreateApiKey))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await apiKeyController.CreateApiKey(apiKeyCreator);

            apiKeyService.Verify(x => x.CreateAsync(It.IsAny<ApiKey>()), Times.Once);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(apiKeyController.FindOne), createdAtActionResult.ActionName);
            var apiKey = Assert.IsType<ApiKey>(createdAtActionResult.Value);
            Assert.NotNull(apiKey);
            Assert.Equal("Id1", apiKey.Id);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task UpdateApiKey()
        {
            ApiKey apiKey = new() { Id = "Id1", Name = "ApiKey 1" };
            var apiKeyService = new Mock<IApiKeyService>();
            var daprClient = new Mock<DaprClient>();

            apiKeyService.Setup(x => x.UpdateAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult((ApiKey?)new ApiKey() { Id = "Id1", Name = "ApiKey 1" })
            );

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            apiKeyController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var authorizeAttributes = apiKeyController.GetType().GetMethod(nameof(ApiKeyController.UpdateApiKey))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await apiKeyController.UpdateApiKey("Id1", apiKey);

            apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
            apiKeyService.Verify(x => x.UpdateAsync(It.IsAny<ApiKey>()), Times.Once);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(apiKeyController.FindOne), acceptedAtActionResult.ActionName);
            var apiKeyResult = Assert.IsType<ApiKey>(acceptedAtActionResult.Value);
            Assert.NotNull(apiKeyResult);
            Assert.Equal("Id1", apiKeyResult.Id);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task UpdateApiKey_Not_Found_ApiKey()
        {
            ApiKey apiKey = new() { Id = "Id1", Name = "ApiKey 1" };
            var apiKeyService = new Mock<IApiKeyService>();
            var daprClient = new Mock<DaprClient>();

            apiKeyService.Setup(x => x.UpdateAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult((ApiKey?)null)
            );

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            var authorizeAttributes = apiKeyController.GetType().GetMethod(nameof(ApiKeyController.UpdateApiKey))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await apiKeyController.UpdateApiKey("Id1", apiKey);

            apiKeyService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
            apiKeyService.Verify(x => x.UpdateAsync(It.IsAny<ApiKey>()), Times.Once);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
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

            var apiKeyController = new ApiKeyController(
                apiKeyService.Object,
                daprClient.Object
            );

            apiKeyController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await apiKeyController.DeleteApiKey(apiKeyId);
            Assert.IsType<NoContentResult>(result);

            apiKeyService.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
