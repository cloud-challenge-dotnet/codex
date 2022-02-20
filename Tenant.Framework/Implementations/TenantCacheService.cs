using System;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Codex.Tenants.Framework.Resources;
using CodexGrpc.Tenants;
using Dapr.Client;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Codex.Tenants.Framework.Implementations;

public class TenantCacheService : CacheServiceBase<Tenant>, ITenantCacheService
{
    private readonly ILogger<TenantCacheService> _logger;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<TenantFrameworkResource> _sl;
        
    public TenantCacheService(
        ILogger<TenantCacheService> logger,
        IMapper mapper,
        DaprClient daprClient,
        IStringLocalizer<TenantFrameworkResource> sl) : base(daprClient)
    {
        _logger = logger;
        _mapper = mapper;
        _sl = sl;
    }
    
    public override string GetCacheKey(Tenant data) => $"{CacheConstant.Tenant_}{data.Id}";

    public async Task<Tenant> GetTenantAsync(string tenantId)
    {
        try
        {
            string cacheKey = $"{CacheConstant.Tenant_}{tenantId}";
            var tenant = await GetCacheAsync(cacheKey);
            
            if (tenant == null)
            {
                var callInvoker = DaprClient.CreateInvocationInvoker(ApiNameConstant.TenantApi);
                TenantService.TenantServiceClient client = new TenantService.TenantServiceClient(
                    callInvoker
                );
                
                var tenantModel = client.FindOne(new FindOneTenantRequest()
                {
                    Id = tenantId
                });
                
                tenant = _mapper.Map<Tenant>(tenantModel);

                await UpdateCacheAsync(cacheKey, tenant);
                return tenant;
            }

            return tenant;
        }
        catch (Exception exception)
        {
            if (exception is Grpc.Core.RpcException rpcException &&
                rpcException.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                _logger.LogInformation(rpcException, "Tenant not found : '{TenantId}'", tenantId);
                throw new InvalidTenantIdException($"{_sl[TenantFrameworkResource.TenantNotFound]} : '{tenantId}'", "TENANT_NOT_FOUND");
            }

            _logger.LogError(exception, "Unable to find Tenant {TenantId}", tenantId);
            throw new TechnicalException($"{_sl[TenantFrameworkResource.TenantNotFound]} : '{tenantId}'", "TENANT_NOT_FOUND");
        }
    }
}