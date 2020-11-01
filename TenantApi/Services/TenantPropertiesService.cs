using Codex.Core;
using Codex.Core.Exceptions;
using Codex.Tenants.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Services
{
    public class TenantPropertiesService
    {
        public TenantPropertiesService(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        private readonly TenantRepository _tenantRepository;

        public async Task<Tenant?> UpdatePropertiesAsync(string tenantId, TenantProperties tenantProperties)
        {
            return await _tenantRepository.UpdatePropertiesAsync(tenantId, tenantProperties);
        }

        public async Task<Tenant?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values)
        {
            return await _tenantRepository.UpdatePropertyAsync(tenantId, propertyKey, values);
        }

        public async Task<Tenant?> DeletePropertyAsync(string tenantId, string propertyKey)
        {
            return await _tenantRepository.DeletePropertyAsync(tenantId, propertyKey);
        }

        public async Task<TenantProperties?> FindPropertiesAsync(string tenantId)
        {
            return (await _tenantRepository.FindOneAsync(tenantId))?.Properties;
        }

        public async Task<List<string>?> FindPropertyAsync(string tenantId, string propertyKey)
        {
            List<string>? values = null;
            (await _tenantRepository.FindOneAsync(tenantId))?.Properties?.TryGetValue(propertyKey, out values);
            return values;
        }
    }
}
