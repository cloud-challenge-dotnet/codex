using Microsoft.AspNetCore.Http;
using Codex.Tenants.Framework.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations
{
    /// <summary>
    /// Resolve the request header to a tenant identifier
    /// </summary>
    public class HostTenantResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HostTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get the tenant identifier
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string?> GetTenantIdentifierAsync()
        {
            string tenantId = _httpContextAccessor.HttpContext?.Request?.Headers["tenantId"].FirstOrDefault();
            return await Task.FromResult(tenantId);
        }
    }
}
