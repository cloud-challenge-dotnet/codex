using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Security;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Controllers
{
    public class ApiKeyTopicController : ControllerBase
    {
        private readonly ILogger<ApiKeyTopicController> _logger;
        private readonly DaprClient _daprClient;
        private readonly ApiKeyCacheService _apiKeyCacheService;

        public ApiKeyTopicController(
            ILogger<ApiKeyTopicController> logger,
            DaprClient daprClient,
            ApiKeyCacheService apiKeyCacheService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _apiKeyCacheService = apiKeyCacheService;
        }

        [Topic(ConfigConstant.CodexPubSubName, TopicConstant.ApiKey)]
        [HttpPost]
        [Route(TopicConstant.ApiKey)]
        public async Task<IActionResult> ProcessApiKeyTopic([FromBody] TopicData<ApiKey> apiKeyTopicData)
        {
            if (!string.IsNullOrWhiteSpace(apiKeyTopicData.Data.Id))
            {
                _logger.LogInformation($"Receive apiKey topic type {apiKeyTopicData.TopicType} id: {apiKeyTopicData.Data.Id}");
                string cacheKey = $"{CacheConstant.ApiKey_}{apiKeyTopicData.Data.Id}";
                switch (apiKeyTopicData.TopicType)
                {
                    case TopicType.Remove:
                        await _apiKeyCacheService.ClearCacheAsync(_daprClient, cacheKey);
                        break;
                    case TopicType.Modify:
                        await _apiKeyCacheService.UpdateCacheAsync(_daprClient, cacheKey, apiKeyTopicData.Data);
                        break;
                }
            }
            else
            {
                _logger.LogWarning($"Receive apiKey topic without id");
            }
            return Ok();
        }
    }
}
