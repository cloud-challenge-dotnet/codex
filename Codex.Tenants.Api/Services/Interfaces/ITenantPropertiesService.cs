using Codex.Models.Tenants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Services
{
    public interface ITenantPropertiesService
    {
        Task<Tenant?> UpdatePropertiesAsync(string tenantId, TenantProperties tenantProperties);

        Task<Tenant?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values);

        Task<Tenant?> DeletePropertyAsync(string tenantId, string propertyKey);

        Task<TenantProperties?> FindPropertiesAsync(string tenantId);

        Task<List<string>?> FindPropertyAsync(string tenantId, string propertyKey);
    }
}
