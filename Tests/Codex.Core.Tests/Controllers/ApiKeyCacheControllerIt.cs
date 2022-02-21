using System.Threading.Tasks;
using Codex.Core.Cache;
using Codex.Core.Controllers;
using Codex.Core.Models;
using Codex.Models.Security;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Codex.Core.Tests.Controllers;

public class ApiKeyCacheControllerIt : IClassFixture<Fixture>
{
    [Fact]
    public async Task ProcessTenantTopic_with_Tenant_Id_Null()
    {
        var logger = new Mock<ILogger<ApiKeyCacheController>>();
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();

        ApiKey apiKey = new();
        TopicData<ApiKey> apiKeyTopicData = new(TopicType.Modify, apiKey, "global");

        var topicController = new ApiKeyCacheController(
            logger.Object,
            apiKeyCacheService.Object
        );

        var result = await topicController.ProcessReceivedTopic(apiKeyTopicData);

        Assert.IsType<OkResult>(result);

        apiKeyCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<string>()), Times.Never);
        apiKeyCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<string>(), It.IsAny<ApiKey>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTenantTopic_Modify_Topic()
    {
        var logger = new Mock<ILogger<ApiKeyCacheController>>();
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();

        string tenantId = "global";
        ApiKey apiKey = new(id: "global", name: "apiKey_global", roles: new());
        TopicData<ApiKey> apiKeyTopicData = new(TopicType.Modify, apiKey, tenantId);

        var topicController = new ApiKeyCacheController(
            logger.Object,
            apiKeyCacheService.Object
        );

        var result = await topicController.ProcessReceivedTopic(apiKeyTopicData);

        Assert.IsType<OkResult>(result);

        apiKeyCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<string>()), Times.Never);
        apiKeyCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<string>(), It.IsAny<ApiKey>()), Times.Once);
    }

    [Fact]
    public async Task ProcessTenantTopic_Remove_Topic()
    {
        var logger = new Mock<ILogger<ApiKeyCacheController>>();
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();

        string tenantId = "global";
        ApiKey apiKey = new(id: "global", name: "apiKey_global", roles: new());
        TopicData<ApiKey> apiKeyTopicData = new(TopicType.Remove, apiKey, tenantId);

        var topicController = new ApiKeyCacheController(
            logger.Object,
            apiKeyCacheService.Object
        );

        var result = await topicController.ProcessReceivedTopic(apiKeyTopicData);

        Assert.IsType<OkResult>(result);

        apiKeyCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<string>()), Times.Once);
        apiKeyCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<string>(), It.IsAny<ApiKey>()), Times.Never);
    }
}