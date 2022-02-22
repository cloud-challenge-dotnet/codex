using Codex.Core.Models;
using Codex.Core.Extensions;
using Dapr.Client;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Core.Cache;

[ExcludeFromCodeCoverage]
public abstract class CacheServiceBase<T> : ICacheService<T>
{
    private readonly int? _expireTimeInSeconds;

    private readonly DaprClient _daprClient;

    protected CacheServiceBase(DaprClient daprClient, int? expireTimeInSeconds = 3600)
    {
        _daprClient = daprClient;
        _expireTimeInSeconds = expireTimeInSeconds;
    }

    public abstract string GetCacheKey(T data);

    public async Task UpdateCacheAsync(string cacheKey, T data)
    {
        Dictionary<string, string>? metadata = _expireTimeInSeconds?.Let(it => new Dictionary<string, string>()
        {
            {"ttlInSeconds", it.ToString()}
        });
        var state = await _daprClient.GetStateEntryAsync<T?>(ConfigConstant.CodexStoreName, cacheKey);
        state.Value = data;
        await state.SaveAsync(metadata: metadata);
    }

    public async Task ClearCacheAsync(string cacheKey)
    {
        await _daprClient.DeleteStateAsync(ConfigConstant.CodexStoreName, cacheKey);
    }
        
    public async Task<T?> GetCacheAsync(string cacheKey)
    {
        var state = await _daprClient.GetStateEntryAsync<T?>(ConfigConstant.CodexStoreName, cacheKey);
        return state == null ? default : state.Value;
    }
}