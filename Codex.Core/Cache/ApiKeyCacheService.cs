using System;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Models;
using Codex.Models.Security;
using CodexGrpc.Security;
using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Codex.Core.Cache;

public class ApiKeyCacheService : CacheServiceBase<ApiKey>, IApiKeyCacheService
{
    private readonly ILogger<ApiKeyCacheService> _logger;
    
    private readonly IMapper _mapper;
    
    public ApiKeyService.ApiKeyServiceClient ApiKeyServiceClient { get; internal set; }
    
    public ApiKeyCacheService(
        DaprClient daprClient,
        int? expireTimeInSeconds,
        ILogger<ApiKeyCacheService> logger,
        IMapper mapper) : base(daprClient, expireTimeInSeconds)
    {
        _logger = logger;
        _mapper = mapper;
        
        // ReSharper disable once VirtualMemberCallInConstructor
        ApiKeyServiceClient = ConstructApiKeyServiceClient();
    }

    private ApiKeyService.ApiKeyServiceClient ConstructApiKeyServiceClient()
    {
        var callInvoker = DaprClient.CreateInvocationInvoker(ApiNameConstant.SecurityApi);
        return new ApiKeyService.ApiKeyServiceClient(
            callInvoker
        );
    }

    public override string GetCacheKey(ApiKey data) => $"{CacheConstant.ApiKey_}{data.Id}";
    
    private string GetCacheKey(string providedApiKey) => $"{CacheConstant.ApiKey_}{providedApiKey}";
    
    public async Task<ApiKey> GetApiKeyAsync(string providedApiKey, string tenantId)
    {
        try
        {
            string cacheKey = GetCacheKey(providedApiKey);
            var apiKey = await GetCacheAsync(cacheKey);
            if (apiKey == null)
            {
                var apiKeyModel = await ApiKeyServiceClient.FindOneAsync(
                    new FindOneApiKeyRequest(){ Id = providedApiKey},
                    new Metadata{
                        new (HttpHeaderConstant.TenantId, tenantId)
                    }
                );
                
                apiKey = _mapper.Map<ApiKey>(apiKeyModel);

                await UpdateCacheAsync(cacheKey, apiKey);
            }

            return apiKey;
        }
        catch (Exception exception)
        {
            if (exception is RpcException rpcException &&
                rpcException.Status.StatusCode == StatusCode.NotFound)
            {
                _logger.LogInformation(rpcException, "ApiKey not found : '{ApiKey}'", providedApiKey);
            }
            else
            {
                _logger.LogError(exception, "Unable to find ApiKey {ApiKey}", providedApiKey);
            }

            throw;
        }
    }
}