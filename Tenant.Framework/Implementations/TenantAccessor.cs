using Codex.Models.Tenants;
using Codex.Tenants.Framework.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Codex.Tenants.Framework.Implementations
{
    public class TenantAccessor : ITenantAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Tenant? Tenant => _httpContextAccessor.HttpContext?.GetTenant();
    }
}
