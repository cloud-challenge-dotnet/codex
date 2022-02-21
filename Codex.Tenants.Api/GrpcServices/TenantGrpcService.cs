using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codex.Models.Roles;
using Grpc.Core;
using AutoMapper;
using Codex.Core.Models;
using Codex.Core.Security;
using CodexGrpc.Common;
using CodexGrpc.Tenants;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Services.Interfaces;
using Dapr.Client;
using Google.Protobuf.Collections;

namespace Codex.Tenants.Api.GrpcServices;

public class TenantGrpcService : TenantService.TenantServiceBase
{
    private readonly IMapper _mapper;
    private readonly DaprClient _daprClient;
    private readonly ITenantService _tenantService;
    private readonly ITenantPropertiesService _tenantPropertiesService;

    public TenantGrpcService(
        IMapper mapper,
        DaprClient daprClient,
        ITenantService tenantService,
        ITenantPropertiesService tenantPropertiesService)
    {
        _mapper = mapper;
        _daprClient = daprClient;
        _tenantService = tenantService;
        _tenantPropertiesService = tenantPropertiesService;
    }
    
    public override async Task<TenantModel> FindOne(FindOneTenantRequest request, ServerCallContext context)
    {
        var tenant = await _tenantService.FindOneAsync(request.Id);

        if (tenant == null)
            throw new RpcException(new Status(StatusCode.NotFound, request.Id));

        if (!context.GetHttpContext().User.IsInRole(RoleConstant.TenantManager))
        {
            tenant = tenant with { Properties = null };
        }

        return _mapper.Map<TenantModel>(tenant);
    }

    public override async Task<TenantListResponse> FindAll(FindAllTenantRequest request, ServerCallContext context)
    {
        var tenantList = await _tenantService.FindAllAsync();

        if (!context.GetHttpContext().User.IsInRole(RoleConstant.TenantManager))
        {
            tenantList = tenantList.Select(t => t with { Properties = null }).ToList();
        }

        TenantListResponse response = new();
        response.Tenants.AddRange(tenantList.Select(it => _mapper.Map<TenantModel>(it)));
        return response;
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<TenantModel> Create(TenantModel request, ServerCallContext context)
    {
        var tenant = _mapper.Map<Tenant>(request);
        tenant = await _tenantService.CreateAsync(tenant);

        await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

        return _mapper.Map<TenantModel>(tenant);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<TenantModel> Update(TenantModel request, ServerCallContext context)
    {
        var tenant = _mapper.Map<Tenant>(request);
        tenant = await _tenantService.UpdateAsync(tenant);
        if (tenant == null)
            throw new RpcException(new Status(StatusCode.NotFound, request.Id));

        await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

        return _mapper.Map<TenantModel>(tenant);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<TenantPropertiesResponse> FindProperties(FindPropertiesRequest request, ServerCallContext context)
    {
        var tenantProperties = await _tenantPropertiesService.FindPropertiesAsync(request.TenantId);

        TenantPropertiesResponse response = new();
        response.Properties.Add(_mapper.Map<MapField <string, StringValues>>(tenantProperties));
        return response;
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<TenantModel> UpdateProperties(UpdatePropertiesRequest request, ServerCallContext context)
    {
        var tenantProperties = _mapper.Map<Dictionary<string, List<string>>>(request.Properties);

        var tenant = await _tenantPropertiesService.UpdatePropertiesAsync(request.TenantId, tenantProperties);
        if (tenant == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.TenantId));
        }

        await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

        return _mapper.Map<TenantModel>(tenant);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<TenantModel> UpdateProperty(UpdatePropertyRequest request, ServerCallContext context)
    {
        var tenantPropertyValues = _mapper.Map<List<string>>(request.PropertyValues);

        var tenant = await _tenantPropertiesService.UpdatePropertyAsync(request.TenantId, request.PropertyKey, tenantPropertyValues);
        if (tenant == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.TenantId));
        }

        await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

        return _mapper.Map<TenantModel>(tenant);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<TenantModel> DeleteProperty(DeletePropertyRequest request, ServerCallContext context)
    {
        var tenant = await _tenantPropertiesService.DeletePropertyAsync(request.TenantId, request.PropertyKey);

        if (tenant == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.TenantId));
        }

        await PublishTenantChangeEventAsync(TopicType.Remove, tenant);
        
        return _mapper.Map<TenantModel>(tenant);
    }

    private async Task PublishTenantChangeEventAsync(TopicType topicType, Tenant tenant)
    {
        await _daprClient.PublishEventAsync(ConfigConstant.CodexPubSubName, TopicConstant.Tenant, new TopicData<Tenant>(topicType, tenant, tenant.Id));
    }
}