using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Dapr.Client;
using Dapr.Client.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Utils
{
    public static class TenantTools
    {
        public static async Task<Tenant> SearchTenantByIdAsync(ILogger logger, TenantCacheService tenantCacheService, DaprClient daprClient, string tenantId)
        {
            try
            {
                string cacheKey = $"{CacheConstant.Tenant_}{tenantId}";
                var tenant = await tenantCacheService.GetCacheAsync(daprClient, cacheKey);

                if (tenant == null)
                {
                    var secretValues = await daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.MicroserviceApiKey);
                    var microserviceApiKey = secretValues[ConfigConstant.MicroserviceApiKey];

                    tenant = await daprClient.InvokeMethodAsync<Tenant>(ApiNameConstant.TenantApi, $"Tenant/{tenantId}",
                        new HTTPExtension()
                        {
                            Verb = HTTPVerb.Get,
                            Headers = {
                                { HttpHeaderConstant.TenantId, tenantId },
                                { HttpHeaderConstant.ApiKey, $"{tenantId}.{microserviceApiKey}" }
                            }
                        }
                    );
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
                    throw new InvalidTenantIdException($"Tenant not found : '{tenantId}'", "TENANT_NOT_FOUND");
                }

                logger.LogError(exception, $"Unable to find Tenant {tenantId}");
                throw new TechnicalException($"Tenant not found : '{tenantId}'", "TENANT_NOT_FOUND");
            }
        }
    }
}
