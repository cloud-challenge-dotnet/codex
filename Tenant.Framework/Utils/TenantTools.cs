using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Codex.Tenants.Framework.Resources;
using Dapr.Client;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Utils
{
    public static class TenantTools
    {
        public static async Task<Tenant> SearchTenantByIdAsync(
            ILogger logger,
            IStringLocalizer<TenantFrameworkResource> sl,
            TenantCacheService tenantCacheService,
            DaprClient daprClient,
            string tenantId)
        {
            try
            {
                string cacheKey = $"{CacheConstant.Tenant_}{tenantId}";
                var tenant = await tenantCacheService.GetCacheAsync(daprClient, cacheKey);

                if (tenant == null)
                {
                    var secretValues = await daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.MicroserviceApiKey);
                    var microserviceApiKey = secretValues[ConfigConstant.MicroserviceApiKey];

                    var request = daprClient.CreateInvokeMethodRequest(ApiNameConstant.TenantApi, $"Tenant/{tenantId}");
                    request.Method = HttpMethod.Get;
                    request.Headers.Add(HttpHeaderConstant.TenantId, tenantId);
                    request.Headers.Add(HttpHeaderConstant.ApiKey, $"{tenantId}.{microserviceApiKey}");
                    tenant = await daprClient.InvokeMethodAsync<Tenant>(request);

                    await tenantCacheService.UpdateCacheAsync(daprClient, cacheKey, tenant);
                    return tenant;
                }
                else
                {
                    return tenant;
                }
            }
            catch (Exception exception)
            {
                if (exception is Grpc.Core.RpcException rpcException &&
                    rpcException.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    logger.LogInformation(rpcException, $"Tenant not found : '{tenantId}'");
                    throw new InvalidTenantIdException($"{sl[TenantFrameworkResource.TENANT_NOT_FOUND]!} : '{tenantId}'", "TENANT_NOT_FOUND");
                }

                logger.LogError(exception, $"Unable to find Tenant {tenantId}");
                throw new TechnicalException($"{sl[TenantFrameworkResource.TENANT_NOT_FOUND]!} : '{tenantId}'", "TENANT_NOT_FOUND");
            }
        }
    }
}
