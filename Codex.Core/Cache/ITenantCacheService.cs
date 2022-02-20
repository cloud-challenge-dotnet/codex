using System.Threading.Tasks;
using Codex.Models.Tenants;

namespace Codex.Core.Cache;

public interface ITenantCacheService : ICacheService<Tenant>
{
    public Task<Tenant> GetTenantAsync(string tenantId);
}