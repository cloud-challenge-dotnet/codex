using Codex.Tenants.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Services
{
    public interface ITenantService
    {
        Task<List<Tenant>> FindAllAsync();

        Task<Tenant?> FindOneAsync(string id);

        Task<Tenant> CreateAsync(TenantCreator tenant);

        Task<Tenant?> UpdateAsync(Tenant tenant);
    }
}
