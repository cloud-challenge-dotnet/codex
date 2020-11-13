using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Models;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations
{
    [ExcludeFromCodeCoverage]
    public class InMemoryTenantStore : ITenantStore
    {
        /// <summary>
        /// Get a tenant for a given identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<Tenant?> GetTenantAsync(string identifier)
        {
            var tenant = new[]
            {
                new Tenant("global", "global", "aazzzsq"),
                new Tenant("demo", "demo", "dsfsdfsdf")
            }.SingleOrDefault(t => t.Id == identifier);

            return await Task.FromResult(tenant);
        }
    }
}
