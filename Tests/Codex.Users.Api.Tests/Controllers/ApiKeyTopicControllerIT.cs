using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Security;
using Codex.Tests.Framework;
using Codex.Users.Api.Controllers;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Users.Api.Tests
{
    public class ApiKeyTopicControllerIT : IClassFixture<Fixture>
    {
        public ApiKeyTopicControllerIT()
        {
        }

        [Fact]
        public async Task ProcessTenantTopic_with_Tenant_Id_Null()
        {
            var logger = new Mock<ILogger<ApiKeyTopicController>>();
            var daprClient = new Mock<DaprClient>();
            var apiKeyCacheService = new Mock<ApiKeyCacheService>();

            ApiKey apiKey = new();
            TopicData<ApiKey> apiKeyTopicData = new(TopicType.Modify, apiKey, "global");

            var topicController = new ApiKeyTopicController(
                logger.Object,
                daprClient.Object,
                apiKeyCacheService.Object
            );

            var result = await topicController.ProcessApiKeyTopic(apiKeyTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            apiKeyCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Never);
            apiKeyCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>(), It.IsAny<ApiKey>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTenantTopic_Modify_Topic()
        {
            var logger = new Mock<ILogger<ApiKeyTopicController>>();
            var daprClient = new Mock<DaprClient>();
            var apiKeyCacheService = new Mock<ApiKeyCacheService>();

            string tenantId = "global";
            ApiKey apiKey = new(id: "global", name: "apiKey_global", roles: new());
            TopicData<ApiKey> apiKeyTopicData = new(TopicType.Modify, apiKey, tenantId);

            var topicController = new ApiKeyTopicController(
                logger.Object,
                daprClient.Object,
                apiKeyCacheService.Object
            );

            var result = await topicController.ProcessApiKeyTopic(apiKeyTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            apiKeyCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Never);
            apiKeyCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>(), It.IsAny<ApiKey>()), Times.Once);
        }

        [Fact]
        public async Task ProcessTenantTopic_Remove_Topic()
        {
            var logger = new Mock<ILogger<ApiKeyTopicController>>();
            var daprClient = new Mock<DaprClient>();
            var apiKeyCacheService = new Mock<ApiKeyCacheService>();

            string tenantId = "global";
            ApiKey apiKey = new(id: "global", name: "apiKey_global", roles: new());
            TopicData<ApiKey> apiKeyTopicData = new(TopicType.Remove, apiKey, tenantId);

            var topicController = new ApiKeyTopicController(
                logger.Object,
                daprClient.Object,
                apiKeyCacheService.Object
            );

            var result = await topicController.ProcessApiKeyTopic(apiKeyTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            apiKeyCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Once);
            apiKeyCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>(), It.IsAny<ApiKey>()), Times.Never);
        }
    }
}
