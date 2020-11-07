using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Core.Interfaces;
using Codex.Tenants.Models;

namespace Codex.Tenants.Api
{
    public interface ITenantRepository: IRepository<Tenant>
    {
        Task<List<Tenant>> FindAllAsync();

        Task<Tenant?> UpdateAsync(Tenant tenant);

        Task<Tenant?> UpdateKeyAsync(string tenantId, string tenantKey);

        Task<Tenant?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values);
        Task<Tenant?> UpdatePropertiesAsync(string tenantId, TenantProperties tenantProperties);

        Task<Tenant?> DeletePropertyAsync(string tenantId, string propertyKey);
    }
}
