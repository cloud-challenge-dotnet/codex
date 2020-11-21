using Codex.Core.Exceptions;
using Codex.Tenants.Framework.Exceptions;
using Codex.Models.Tenants;
using Dapr.Client;
using Dapr.Client.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Utils
{
    public static class MicroServiceTenantTools
    {
        public static async Task<Tenant> SearchTenantByIdAsync(ILogger logger, DaprClient daprClient, string tenantId)
        {
            try
            {
                return await daprClient.InvokeMethodAsync<Tenant>("tenantapi", $"Tenant/{tenantId}", new HTTPExtension() { Verb = HTTPVerb.Get });
            }
            catch (Exception exception)
            {
                if (exception is Grpc.Core.RpcException rpcException)
                {
                    if (rpcException.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
                    {
                        logger.LogInformation(rpcException, $"Tenant not found : '{tenantId}'");
                        throw new InvalidTenantIdException($"Tenant not found : '{tenantId}'", "TENANT_NOT_FOUND");
                    }
                }

                logger.LogError(exception, $"Unable to find Tenant {tenantId}");
                throw new TechnicalException($"Tenant not found : '{tenantId}'", "TENANT_NOT_FOUND");
            }
        }
    }
}
