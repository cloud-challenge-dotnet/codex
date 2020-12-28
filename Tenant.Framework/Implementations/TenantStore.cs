﻿using Codex.Core.Cache;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Utils;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations
{
    [ExcludeFromCodeCoverage]
    public class TenantStore : ITenantStore
    {
        private readonly ILogger<TenantStore> _logger;

        private readonly DaprClient _daprClient;

        private readonly TenantCacheService _tenantCacheService;

        public TenantStore(
            ILogger<TenantStore> logger,
            DaprClient daprClient,
            TenantCacheService tenantCacheService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _tenantCacheService = tenantCacheService;
        }

        /// <summary>
        /// Get a tenant for a given identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public async Task<Tenant?> GetTenantAsync(string identifier)
        {
            Tenant tenant = await TenantTools.SearchTenantByIdAsync(_logger, _tenantCacheService, _daprClient, identifier);

            return await Task.FromResult(tenant);
        }
    }
}
