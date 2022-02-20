using Codex.Models.Tenants;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Interfaces;

public interface ITenantAccessService
{

    /// <summary>
    /// Get the current tenant
    /// </summary>
    /// <returns></returns>
    Task<Tenant?> GetTenantAsync(string? tenantIdentifier = null);
}