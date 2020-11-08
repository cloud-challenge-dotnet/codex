using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tests.Framework
{
    [ExcludeFromCodeCoverage]
    public class TestTenantAccessService : ITenantAccessService
    {
        private readonly string _testId = Guid.NewGuid().ToString();

        public Task<Tenant?> GetTenantAsync()
        {
            return Task.FromResult<Tenant?>(new Tenant(_testId, "Test", ""));
        }
    }
}
