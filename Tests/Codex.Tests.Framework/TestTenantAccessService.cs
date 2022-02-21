using Codex.Models.Tenants;
using Codex.Tenants.Framework.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tests.Framework;

[ExcludeFromCodeCoverage]
public class TestTenantAccessService : ITenantAccessService
{
    private readonly string _testId = Guid.NewGuid().ToString();

    public Task<Tenant?> GetTenantAsync(string? tenantIdentifier = null)
    {
        return Task.FromResult<Tenant?>(new Tenant(tenantIdentifier??_testId, "Test"));
    }
}