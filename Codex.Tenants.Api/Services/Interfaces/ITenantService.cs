using Codex.Models.Tenants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Services
{
    public interface ITenantService
    {
        Task<List<Tenant>> FindAllAsync();

        Task<Tenant?> FindOneAsync(string id);

        Task<Tenant> CreateAsync(Tenant tenant);

        Task<Tenant?> UpdateAsync(Tenant tenant);
    }
}
