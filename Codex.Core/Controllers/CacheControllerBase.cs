using Codex.Core.Cache;
using Codex.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Codex.Core.Controllers;

public abstract class CacheControllerBase<T> : ControllerBase
{
    protected readonly ILogger<CacheControllerBase<T>> Logger;
    private readonly ICacheService<T> _cacheService;

    protected CacheControllerBase(
        ILogger<CacheControllerBase<T>> logger,
        ICacheService<T> cacheService)
    {
        Logger = logger;
        _cacheService = cacheService;
    }

    public virtual async Task<IActionResult> ProcessReceivedTopic([FromBody] TopicData<T> topicData)
    {
        Logger.LogInformation("Receive topic type {TopicType} topic", typeof(T).Name);
        string cacheKey = _cacheService.GetCacheKey(topicData.Data);
        switch (topicData.TopicType)
        {
            case TopicType.Remove:
                await _cacheService.ClearCacheAsync(cacheKey);
                break;
            case TopicType.Modify:
                await _cacheService.UpdateCacheAsync(cacheKey, topicData.Data);
                break;
        }
        return Ok();
    }
}
