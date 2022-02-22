using Codex.Models.Tenants;
using Codex.Tenants.Framework.Interfaces;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations;

/// <summary>
/// Tenant access service
/// </summary>
public class TenantAccessService : ITenantAccessService
{
    private readonly ITenantResolutionStrategy _tenantResolutionStrategy;
    private readonly ITenantStore _tenantStore;

    public TenantAccessService(ITenantResolutionStrategy tenantResolutionStrategy, ITenantStore tenantStore)
    {
        _tenantResolutionStrategy = tenantResolutionStrategy;
        _tenantStore = tenantStore;
    }

    /// <summary>
    /// Get the current tenant
    /// </summary>
    /// <returns></returns>
    public async Task<Tenant?> GetTenantAsync(string? tenantIdentifier = null)
    {
        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            tenantIdentifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
        }

        if (string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            return null;
        }
        return await _tenantStore.GetTenantAsync(tenantIdentifier);
    }
}