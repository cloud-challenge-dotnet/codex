using System.Threading.Tasks;

namespace Codex.Core.Cache;

public interface ICacheService<T>
{
    public string GetCacheKey(T data);
    
    public Task UpdateCacheAsync(string cacheKey, T data);

    public Task ClearCacheAsync(string cacheKey);

    public Task<T?> GetCacheAsync(string cacheKey);
}