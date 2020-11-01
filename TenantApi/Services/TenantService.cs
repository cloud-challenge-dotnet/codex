using Codex.Core;
using Codex.Core.Exceptions;
using Codex.Tenants.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Services
{
    public class TenantService
    {
        public TenantService(TenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        private readonly TenantRepository _tenantRepository;

        public async Task<List<Tenant>> FindAllAsync()
        {
            return await _tenantRepository.FindAllAsync();
        }

        public async Task<Tenant> FindOneAsync(string id)
        {
            return await _tenantRepository.FindOneAsync(id);
        }

        public async Task<Tenant> CreateAsync(TenantCreator tenant)
        {
            var tenantId = tenant.Id ?? throw new ArgumentException("Tenant id is mandatory");

            if (await _tenantRepository.ExistsByIdAsync(tenantId))
            {
                throw new IllegalArgumentException(code: "TENANT_EXISTS", message: $"Tenant {tenantId} already exists");
            }
            return await _tenantRepository.InsertAsync(tenant.ToTenant() with { Key = StringUtils.RandomString(8) });
        }

        public async Task<Tenant> UpdateAsync(Tenant tenant)
        {
            return await _tenantRepository.UpdateAsync(tenant);
        }
    }
}
