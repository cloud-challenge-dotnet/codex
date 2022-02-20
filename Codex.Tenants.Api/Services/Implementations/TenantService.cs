using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Extensions;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Tenants.Api.Repositories.Models;
using Codex.Tenants.Api.Resources;
using Codex.Tenants.Api.Services.Interfaces;
using Microsoft.Extensions.Localization;

namespace Codex.Tenants.Api.Services.Implementations;

public class TenantService : ITenantService
{
    private readonly IMapper _mapper;

    private readonly IStringLocalizer<TenantResource> _sl;

    public TenantService(ITenantRepository tenantRepository,
        IMapper mapper,
        IStringLocalizer<TenantResource> sl)
    {
        _tenantRepository = tenantRepository;
        _mapper = mapper;
        _sl = sl;
    }

    private readonly ITenantRepository _tenantRepository;

    public async Task<List<Tenant>> FindAllAsync()
    {
        var tenantRows = await _tenantRepository.FindAllAsync();

        return tenantRows.Select(it => _mapper.Map<Tenant>(it)).ToList();
    }

    public async Task<Tenant?> FindOneAsync(string id)
    {
        var tenantRow = await _tenantRepository.FindOneAsync(id);
        return tenantRow?.Let(it => _mapper.Map<Tenant>(it));
    }
    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        var tenantId = !string.IsNullOrWhiteSpace(tenant.Id) ? tenant.Id : throw new ArgumentException(_sl[TenantResource.TenantIdIsMandatory]);

        if (await _tenantRepository.ExistsByIdAsync(tenantId))
        {
            throw new IllegalArgumentException(code: "TENANT_EXISTS", message: string.Format(_sl[TenantResource.TenantP0AlreadyExists], tenantId));
        }
        var tenantRow = await _tenantRepository.InsertAsync(_mapper.Map<TenantRow>(tenant));
        return _mapper.Map<Tenant>(tenantRow);
    }

    public async Task<Tenant?> UpdateAsync(Tenant tenant)
    {
        var tenantRow = await _tenantRepository.UpdateAsync(_mapper.Map<TenantRow>(tenant));
        return tenantRow?.Let(it => _mapper.Map<Tenant>(it));
    }
}