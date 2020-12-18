using Codex.Core;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Interfaces;
using Codex.Security.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Security.Api.Services.Implementations
{
    public class ApiKeyService : IApiKeyService
    {
        public ApiKeyService(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository;
        }

        private readonly IApiKeyRepository _apiKeyRepository;

        public async Task<List<ApiKey>> FindAllAsync(ApiKeyCriteria apiKeyCriteria)
        {
            return await _apiKeyRepository.FindAllAsync(apiKeyCriteria);
        }

        public async Task<ApiKey?> FindOneAsync(string id)
        {
            return await _apiKeyRepository.FindOneAsync(id);
        }

        public async Task<ApiKey> CreateAsync(ApiKey apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey.Id))
            {
                apiKey.Id = StringUtils.RandomString(50);
            }

            return await _apiKeyRepository.InsertAsync(apiKey);
        }

        public async Task<ApiKey?> UpdateAsync(ApiKey apiKey)
        {
            return await _apiKeyRepository.UpdateAsync(apiKey);
        }

        public async Task DeleteAsync(string apiKeyId)
        {
            await _apiKeyRepository.DeleteAsync(apiKeyId);
        }
    }
}
