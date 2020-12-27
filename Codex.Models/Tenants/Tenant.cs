using System.Collections.Generic;

namespace Codex.Models.Tenants
{
    public record Tenant(
        string? Id = null,
        string Name = "",
        TenantProperties? Properties = null
    );

    public class TenantProperties : Dictionary<string, List<string>>
    {
    }
}
