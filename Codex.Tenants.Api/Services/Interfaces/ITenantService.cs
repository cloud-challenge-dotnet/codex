using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Models.Tenants;

namespace Codex.Tenants.Api.Services.Interfaces;

public interface ITenantService
{
    Task<List<Tenant>> FindAllAsync();

    Task<Tenant?> FindOneAsync(string id);

    Task<Tenant> CreateAsync(Tenant tenant);

    Task<Tenant?> UpdateAsync(Tenant tenant);
}