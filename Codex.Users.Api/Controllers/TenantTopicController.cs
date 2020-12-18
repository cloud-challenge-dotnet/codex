using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Tenants;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
    public class TenantTopicController : ControllerBase
    {
        private readonly ILogger<TenantTopicController> _logger;
        private readonly DaprClient _daprClient;
        private readonly TenantCacheService _tenantCacheService;

        public TenantTopicController(
            ILogger<TenantTopicController> logger,
            DaprClient daprClient,
            TenantCacheService tenantCacheService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _tenantCacheService = tenantCacheService;
        }

        [Topic(ConfigConstant.CodexPubSubName, TopicConstant.Tenant)]
        [HttpPost]
        [Route(TopicConstant.Tenant)]
        public async Task<IActionResult> ProcessTenantTopic([FromBody] TopicData<Tenant> tenantTopicData)
        {
            if (!string.IsNullOrWhiteSpace(tenantTopicData.Data.Id))
            {
                _logger.LogInformation($"Receive tenant topic type {tenantTopicData.TopicType} id: {tenantTopicData.Data.Id}");
                string cacheKey = $"{CacheConstant.Tenant_}{tenantTopicData.Data.Id}";
                switch (tenantTopicData.TopicType)
                {
                    case TopicType.Remove:
                        await _tenantCacheService.ClearCacheAsync(_daprClient, cacheKey);
                        break;
                    case TopicType.Modify:
                        await _tenantCacheService.UpdateCacheAsync(_daprClient, cacheKey, tenantTopicData.Data);
                        break;
                }
            }
            else
            {
                _logger.LogWarning($"Receive tenant topic without id");
            }
            return Ok();
        }
    }
}
