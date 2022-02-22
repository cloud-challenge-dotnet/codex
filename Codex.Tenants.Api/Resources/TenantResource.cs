using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Api.Resources;

[ExcludeFromCodeCoverage]
public class TenantResource
{
    public const string TenantIdIsMandatory = nameof(TenantIdIsMandatory);
    public const string TenantP0AlreadyExists = nameof(TenantP0AlreadyExists);
}