using Codex.Models.Tenants;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Dapr.Client;
using System.Threading;
using Codex.Users.Api.Controllers;
using Codex.Core.Models;
using Microsoft.Extensions.Logging;
using Codex.Core.Cache;

namespace Codex.Users.Api.Tests
{
    public class TopicControllerIT : IClassFixture<Fixture>
    {
        public TopicControllerIT()
        {
        }

        [Fact]
        public async Task ProcessTenantTopic_with_Tenant_Id_Null()
        {
            var logger = new Mock<ILogger<TopicController>>();
            var daprClient = new Mock<DaprClient>();
            var tenantCacheService = new Mock<CacheService<Tenant>>();

            Tenant tenant = new();
            TopicData<Tenant> tenantTopicData = new(TopicType.Modify, tenant);

            var topicController = new TopicController(
                logger.Object,
                daprClient.Object,
                tenantCacheService.Object
            );

            var result = await topicController.ProcessTenantTopic(tenantTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            tenantCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Never);
            tenantCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>(), It.IsAny<Tenant>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTenantTopic_Modify_Topic()
        {
            var logger = new Mock<ILogger<TopicController>>();
            var daprClient = new Mock<DaprClient>();
            var tenantCacheService = new Mock<CacheService<Tenant>>();

            Tenant tenant = new(id: "global", name: "tenant_global");
            TopicData<Tenant> tenantTopicData = new(TopicType.Modify, tenant);

            var topicController = new TopicController(
                logger.Object,
                daprClient.Object,
                tenantCacheService.Object
            );

            var result = await topicController.ProcessTenantTopic(tenantTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            tenantCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Never);
            tenantCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>(), It.IsAny<Tenant>()), Times.Once);
        }

        [Fact]
        public async Task ProcessTenantTopic_Remove_Topic()
        {
            var logger = new Mock<ILogger<TopicController>>();
            var daprClient = new Mock<DaprClient>();
            var tenantCacheService = new Mock<CacheService<Tenant>>();

            Tenant tenant = new(id: "global", name: "tenant_global");
            TopicData<Tenant> tenantTopicData = new(TopicType.Remove, tenant);

            var topicController = new TopicController(
                logger.Object,
                daprClient.Object,
                tenantCacheService.Object
            );

            var result = await topicController.ProcessTenantTopic(tenantTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            tenantCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Once);
            tenantCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>(), It.IsAny<Tenant>()), Times.Never);
        }
    }
}
