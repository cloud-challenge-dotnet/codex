using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Extensions;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Tenants.Api.Services.Interfaces;

namespace Codex.Tenants.Api.Services.Implementations;

public class TenantPropertiesService : ITenantPropertiesService
{
    private readonly IMapper _mapper;

    public TenantPropertiesService(ITenantRepository tenantRepository,
        IMapper mapper)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
    }

    private readonly ITenantRepository _tenantRepository;

    public async Task<Tenant?> UpdatePropertiesAsync(string tenantId, Dictionary<string, List<string>> tenantProperties)
    {
        var tenantRow = await _tenantRepository.UpdatePropertiesAsync(tenantId, tenantProperties);
        return tenantRow?.Let(it => _mapper.Map<Tenant>(it));
    }

    public async Task<Tenant?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values)
    {
        var tenantRow = await _tenantRepository.UpdatePropertyAsync(tenantId, propertyKey, values);
        return tenantRow?.Let(it => _mapper.Map<Tenant>(it));
    }

    public async Task<Tenant?> DeletePropertyAsync(string tenantId, string propertyKey)
    {
        var tenantRow = await _tenantRepository.DeletePropertyAsync(tenantId, propertyKey);
        return tenantRow?.Let(it => _mapper.Map<Tenant>(it));
    }

    public async Task<Dictionary<string, List<string>>?> FindPropertiesAsync(string tenantId)
    {
        var tenantRow = await _tenantRepository.FindOneAsync(tenantId);
        return tenantRow?.Properties?.Let(it => _mapper.Map<Dictionary<string, List<string>>?>(it));
    }

    public async Task<List<string>?> FindPropertyAsync(string tenantId, string propertyKey)
    {
        List<string>? values = null;
        (await _tenantRepository.FindOneAsync(tenantId))?.Properties?.TryGetValue(propertyKey, out values);
        return values;
    }
}