using Codex.Models.Tenants;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Interfaces;

public interface ITenantStore
{
    Task<Tenant?> GetTenantAsync(string identifier);
}