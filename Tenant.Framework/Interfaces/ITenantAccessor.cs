using Codex.Tenants.Models;

namespace Codex.Tenants.Framework.Interfaces
{
    public interface ITenantAccessor
    {
        Tenant? Tenant { get; }
    }
}
