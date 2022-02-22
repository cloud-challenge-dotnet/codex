using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Security;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Codex.Core.Controllers;

public class ApiKeyCacheController : CacheControllerBase<ApiKey>
{
    public ApiKeyCacheController(
        ILogger<ApiKeyCacheController> logger,
        IApiKeyCacheService cacheService) : base(logger, cacheService)
    {
    }

    [Topic(ConfigConstant.CodexPubSubName, TopicConstant.ApiKey)]
    [HttpPost]
    [Route(TopicConstant.ApiKey)]
    public override async Task<IActionResult> ProcessReceivedTopic([FromBody] TopicData<ApiKey> topicData)
    {
        if (!string.IsNullOrWhiteSpace(topicData.Data.Id))
        {
            return await base.ProcessReceivedTopic(topicData);
        }
        else
        {
            Logger.LogWarning("Receive apiKey topic without id");
        }
        return Ok();
    }
}
