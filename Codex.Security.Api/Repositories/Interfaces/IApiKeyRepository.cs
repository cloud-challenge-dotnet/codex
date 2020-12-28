using Codex.Core.Interfaces;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Security.Api.Repositories.Interfaces
{
    public interface IApiKeyRepository : IRepository<ApiKeyRow, string>
    {
        Task<List<ApiKeyRow>> FindAllAsync(ApiKeyCriteria apiKeyCriteria);

        Task<ApiKeyRow?> UpdateAsync(ApiKeyRow apiKey);

        Task DeleteAsync(string apiKeyId);
    }
}
