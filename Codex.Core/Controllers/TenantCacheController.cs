using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Tenants;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Codex.Core.Controllers;

public class TenantCacheController : CacheControllerBase<Tenant>
{
    public TenantCacheController(
        ILogger<TenantCacheController> logger,
        ITenantCacheService cacheService) : base(logger, cacheService)
    {
    }

    [Topic(ConfigConstant.CodexPubSubName, TopicConstant.Tenant)]
    [HttpPost]
    [Route(TopicConstant.ApiKey)]
    public override async Task<IActionResult> ProcessReceivedTopic([FromBody] TopicData<Tenant> topicData)
    {
        if (!string.IsNullOrWhiteSpace(topicData.Data.Id))
        {
            return await base.ProcessReceivedTopic(topicData);
        }
        else
        {
            Logger.LogWarning("Receive tenant topic without id");
        }
        return Ok();
    }
}
