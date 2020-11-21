using Codex.Models.Tenants;

namespace Codex.Tenants.Framework.Interfaces
{
    public interface ITenantAccessor
    {
        Tenant? Tenant { get; }
    }
}
