using Codex.Models.Security;

namespace Codex.Core.Cache
{
    public class ApiKeyCacheService : CacheService<ApiKey>
    {
        public ApiKeyCacheService() : base(expireTimeInMinutes: 60)
        {
        }
    }
}
