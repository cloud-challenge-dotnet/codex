using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Models;
using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Models.Security;
using Codex.Security.Api.Services.Interfaces;
using Codex.Tenants.Framework;
using CodexGrpc.Security;
using CodexGrpc.Tenants;
using Dapr.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Codex.Security.Api.GrpcServices;

public class ApiKeyGrpcService : ApiKeyService.ApiKeyServiceBase
{
    private readonly IMapper _mapper;
    private readonly DaprClient _daprClient;
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyGrpcService(
        IMapper mapper,
        DaprClient daprClient,
        IApiKeyService apiKeyService)
    {
        _mapper = mapper;
        _daprClient = daprClient;
        _apiKeyService = apiKeyService;
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<ApiKeyModel> FindOne(FindOneApiKeyRequest request, ServerCallContext context)
    {
        var apiKey = await _apiKeyService.FindOneAsync(request.Id);
        
        if (apiKey == null)
            throw new RpcException(new Status(StatusCode.NotFound, request.Id));

        return _mapper.Map<ApiKeyModel>(apiKey);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<ApiKeyListResponse> FindAll(FindAllApiKeyRequest request, ServerCallContext context)
    {
        var apiKeyCriteria = _mapper.Map<ApiKeyCriteria>(request.Criteria);
        var apiKeyList = await _apiKeyService.FindAllAsync(apiKeyCriteria);

        ApiKeyListResponse response = new();
        response.ApiKeys.AddRange(apiKeyList.Select(it => _mapper.Map<ApiKeyModel>(it)));
        return response;
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<ApiKeyModel> Create(ApiKeyModel request, ServerCallContext context)
    {
        var apiKey = _mapper.Map<ApiKey>(request);
        apiKey = await _apiKeyService.CreateAsync(apiKey);
        
        var tenant = context.GetHttpContext().GetTenant();
        await PublishApiKeyChangeEventAsync(TopicType.Modify, apiKey, tenant!.Id);
        
        return _mapper.Map<ApiKeyModel>(apiKey);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<ApiKeyModel> Update(ApiKeyModel request, ServerCallContext context)
    {
        var apiKey = _mapper.Map<ApiKey>(request);
        apiKey = await _apiKeyService.UpdateAsync(apiKey);
        
        if (apiKey == null)
            throw new RpcException(new Status(StatusCode.NotFound, request.Id));
        
        var tenant = context.GetHttpContext().GetTenant();
        await PublishApiKeyChangeEventAsync(TopicType.Modify, apiKey, tenant!.Id);
        
        return _mapper.Map<ApiKeyModel>(apiKey);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<Empty> Delete(DeleteApiKeyRequest request, ServerCallContext context)
    {
        await _apiKeyService.DeleteAsync(request.Id);

        var tenant = context.GetHttpContext().GetTenant();
        await PublishApiKeyChangeEventAsync(TopicType.Remove, new ApiKey() { Id = request.Id }, tenant!.Id);

        return new Empty();
    }
    
    private async Task PublishApiKeyChangeEventAsync(TopicType topicType, ApiKey apiKey, string tenantId)
    {
        await _daprClient.PublishEventAsync(ConfigConstant.CodexPubSubName, TopicConstant.ApiKey, new TopicData<ApiKey>(topicType, apiKey, tenantId));
    }
}