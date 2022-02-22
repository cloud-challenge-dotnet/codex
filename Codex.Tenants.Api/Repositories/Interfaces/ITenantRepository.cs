using Codex.Core.Interfaces;
using Codex.Tenants.Api.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Repositories.Interfaces;

public interface ITenantRepository : IRepository<TenantRow, string>
{
    Task<List<TenantRow>> FindAllAsync();

    Task<TenantRow?> UpdateAsync(TenantRow tenant);

    Task<TenantRow?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values);

    Task<TenantRow?> UpdatePropertiesAsync(string tenantId, Dictionary<string, List<string>> tenantProperties);

    Task<TenantRow?> DeletePropertyAsync(string tenantId, string propertyKey);
}