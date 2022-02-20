using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Models.Tenants;

namespace Codex.Tenants.Api.Services.Interfaces;

public interface ITenantPropertiesService
{
    Task<Tenant?> UpdatePropertiesAsync(string tenantId, Dictionary<string, List<string>> tenantProperties);

    Task<Tenant?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values);

    Task<Tenant?> DeletePropertyAsync(string tenantId, string propertyKey);

    Task<Dictionary<string, List<string>>?> FindPropertiesAsync(string tenantId);

    Task<List<string>?> FindPropertyAsync(string tenantId, string propertyKey);
}