using Codex.Models.Tenants;
using Codex.Tenants.Framework.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations;

[ExcludeFromCodeCoverage]
public class GlobalTenantStore : ITenantStore
{
    /// <summary>
    /// Get a tenant for a given identifier
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public async Task<Tenant?> GetTenantAsync(string identifier)
    {
        return await Task.FromResult(new Tenant(Id: "global", Name: "global"));
    }
}