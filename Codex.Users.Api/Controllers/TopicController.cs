﻿using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Utils;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
    public class TopicController : ControllerBase
    {
        private readonly ILogger<TopicController> _logger;
        private readonly DaprClient _daprClient;
        private readonly CacheService<Tenant> _tenantCacheService;

        public TopicController(
            ILogger<TopicController> logger,
            DaprClient daprClient,
            CacheService<Tenant> tenantCacheService)
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
