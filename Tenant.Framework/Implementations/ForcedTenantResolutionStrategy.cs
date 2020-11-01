using Codex.Tenants.Framework.Interfaces;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations
{
    /// <summary>
    /// Resolve the request header to a tenant identifier
    /// </summary>
    public class GlobalTenantResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly string _tenantId = "global";

        public GlobalTenantResolutionStrategy()
        {
        }

        /// <summary>
        /// Get the tenant identifier
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> GetTenantIdentifierAsync()
        {
            return await Task.FromResult(_tenantId);
        }
    }
}
