using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Api.Resources
{
    [ExcludeFromCodeCoverage]
    public class TenantResource
    {
        public const string TENANT_ID_IS_MANDATORY = nameof(TENANT_ID_IS_MANDATORY);
        public const string TENANT_P0_ALREADY_EXISTS = nameof(TENANT_P0_ALREADY_EXISTS);
    }
}
