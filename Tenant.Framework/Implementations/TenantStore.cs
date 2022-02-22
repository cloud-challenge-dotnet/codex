using Codex.Models.Tenants;
using Codex.Tenants.Framework.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Codex.Core.Cache;

namespace Codex.Tenants.Framework.Implementations;

[ExcludeFromCodeCoverage]
public class TenantStore : ITenantStore
{
    private readonly ITenantCacheService _tenantCacheService;

    public TenantStore(ITenantCacheService tenantCacheService)
    {
        _tenantCacheService = tenantCacheService;
    }

    /// <summary>
    /// Get a tenant for a given identifier
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public async Task<Tenant?> GetTenantAsync(string identifier)
    {
        Tenant tenant = await _tenantCacheService.GetTenantAsync(identifier);

        return await Task.FromResult(tenant);
    }
}