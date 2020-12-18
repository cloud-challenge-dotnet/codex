using Codex.Models.Tenants;

namespace Codex.Core.Cache
{
    public class TenantCacheService : CacheService<Tenant>
    {
        public TenantCacheService() : base(expireTimeInMinutes: 60)
        {
        }
    }
}
