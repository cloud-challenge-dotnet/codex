using Codex.Core.Interfaces;
using Codex.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Security.Api.Repositories.Interfaces
{
    public interface IApiKeyRepository : IRepository<ApiKey>
    {
        Task<List<ApiKey>> FindAllAsync(ApiKeyCriteria apiKeyCriteria);

        Task<ApiKey?> UpdateAsync(ApiKey apiKey);

        Task DeleteAsync(string apiKeyId);
    }
}
