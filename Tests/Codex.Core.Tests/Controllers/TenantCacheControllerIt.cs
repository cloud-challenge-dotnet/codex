using System.Threading.Tasks;
using Codex.Core.Cache;
using Codex.Core.Controllers;
using Codex.Core.Models;
using Codex.Models.Tenants;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Codex.Core.Tests.Controllers;

public class TenantCacheControllerIt : IClassFixture<Fixture>
{
    [Fact]
    public async Task ProcessTenantTopic_with_Tenant_Id_Null()
    {
        var logger = new Mock<ILogger<TenantCacheController>>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        Tenant tenant = new();
        TopicData<Tenant> tenantTopicData = new(TopicType.Modify, tenant, "global");

        var topicController = new TenantCacheController(
            logger.Object,
            tenantCacheService.Object
        );

        var result = await topicController.ProcessReceivedTopic(tenantTopicData);

        Assert.IsType<OkResult>(result);

        tenantCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<string>()), Times.Never);
        tenantCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<string>(), It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task ProcessTenantTopic_Modify_Topic()
    {
        var logger = new Mock<ILogger<TenantCacheController>>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        Tenant tenant = new(Id: "global", Name: "tenant_global");
        TopicData<Tenant> tenantTopicData = new(TopicType.Modify, tenant, tenant.Id);

        var topicController = new TenantCacheController(
            logger.Object,
            tenantCacheService.Object
        );

        var result = await topicController.ProcessReceivedTopic(tenantTopicData);

        Assert.IsType<OkResult>(result);

        tenantCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<string>()), Times.Never);
        tenantCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<string>(), It.IsAny<Tenant>()), Times.Once);
    }

    [Fact]
    public async Task ProcessTenantTopic_Remove_Topic()
    {
        var logger = new Mock<ILogger<TenantCacheController>>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        Tenant tenant = new(Id: "global", Name: "tenant_global");
        TopicData<Tenant> tenantTopicData = new(TopicType.Remove, tenant, tenant.Id);

        var topicController = new TenantCacheController(
            logger.Object,
            tenantCacheService.Object
        );

        var result = await topicController.ProcessReceivedTopic(tenantTopicData);

        Assert.IsType<OkResult>(result);

        tenantCacheService.Verify(x => x.ClearCacheAsync(It.IsAny<string>()), Times.Once);
        tenantCacheService.Verify(x => x.UpdateCacheAsync(It.IsAny<string>(), It.IsAny<Tenant>()), Times.Never);
    }
}