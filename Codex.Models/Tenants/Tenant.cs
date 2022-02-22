using System.Collections.Generic;

namespace Codex.Models.Tenants;

public record Tenant(
    string Id = "",
    string Name = "",
    Dictionary<string, List<string>>? Properties = null
);