using AutoMapper;
using Codex.Core.Extensions;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Interfaces;
using Codex.Security.Api.Repositories.Models;
using Codex.Security.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codex.Core.Tools;

namespace Codex.Security.Api.Services.Implementations;

public class ApiKeyService : IApiKeyService
{
    public ApiKeyService(IApiKeyRepository apiKeyRepository,
        IMapper mapper)
    {
        _apiKeyRepository = apiKeyRepository;
        _mapper = mapper;
    }

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IMapper _mapper;

    public async Task<List<ApiKey>> FindAllAsync(ApiKeyCriteria apiKeyCriteria)
    {
        var apiKeyRows = await _apiKeyRepository.FindAllAsync(apiKeyCriteria);

        return apiKeyRows.Select(it => _mapper.Map<ApiKey>(it)).ToList();
    }

    public async Task<ApiKey?> FindOneAsync(string id)
    {
        var apiKeyRow = await _apiKeyRepository.FindOneAsync(id);
        return apiKeyRow?.Let(it => _mapper.Map<ApiKey>(it));
    }

    public async Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey.Id))
        {
            apiKey = apiKey with
            {
                Id = StringUtils.RandomString(50)
            };
        }

        var apiKeyRow = await _apiKeyRepository.InsertAsync(_mapper.Map<ApiKeyRow>(apiKey));
        return _mapper.Map<ApiKey>(apiKeyRow);
    }

    public async Task<ApiKey?> UpdateAsync(ApiKey apiKey)
    {
        var apiKeyRow = await _apiKeyRepository.UpdateAsync(_mapper.Map<ApiKeyRow>(apiKey));
        return apiKeyRow?.Let(it => _mapper.Map<ApiKey>(it));
    }

    public async Task DeleteAsync(string apiKeyId)
    {
        await _apiKeyRepository.DeleteAsync(apiKeyId);
    }
}