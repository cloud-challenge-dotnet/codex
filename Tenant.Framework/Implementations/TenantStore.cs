using Codex.Tenants.Framework.Interfaces;
using Codex.Models.Tenants;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Codex.Tenants.Framework.Utils;

namespace Codex.Tenants.Framework.Implementations
{
    [ExcludeFromCodeCoverage]
    public class TenantStore : ITenantStore
    {
        private readonly ILogger<TenantStore> _logger;

        private readonly DaprClient _daprClient;

        public TenantStore(
            ILogger<TenantStore> logger,
            DaprClient daprClient)
        {
            _logger = logger;

            _daprClient = daprClient;
        }

        /// <summary>
        /// Get a tenant for a given identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<Tenant?> GetTenantAsync(string identifier)
        {
            Tenant tenant = await MicroServiceTenantTools.SearchTenantByIdAsync(_logger, _daprClient, identifier);

            return await Task.FromResult(tenant);
        }
    }
}
