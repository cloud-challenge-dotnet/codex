using Codex.Core.Models;
using Codex.Models.Security;
using Dapr.Client;

namespace Codex.Core.Cache;

public class ApiKeyCacheService : CacheServiceBase<ApiKey>, IApiKeyCacheService
{
    public ApiKeyCacheService(DaprClient daprClient, int? expireTimeInSeconds) : base(daprClient, expireTimeInSeconds)
    {
    }

    public override string GetCacheKey(ApiKey data) => $"{CacheConstant.ApiKey_}{data.Id}";
}