using Codex.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Security.Api.Services.Interfaces
{
    public interface IApiKeyService
    {
        Task<List<ApiKey>> FindAllAsync(ApiKeyCriteria apiKeyCriteria);

        Task<ApiKey?> FindOneAsync(string id);

        Task<ApiKey> CreateAsync(ApiKey apiKey);

        Task<ApiKey?> UpdateAsync(ApiKey apiKey);

        Task DeleteAsync(string apiKeyId);
    }
}
