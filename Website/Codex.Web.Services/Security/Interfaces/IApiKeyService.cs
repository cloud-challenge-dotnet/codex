using Codex.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Web.Services.Security.Interfaces
{
    public interface IApiKeyService
    {
        Task<ApiKey> FindOneAsync(string id);

        Task<List<ApiKey>> FindAllAsync();

        Task<ApiKey> CreateAsync(ApiKey apiKey);

        Task<ApiKey> UpdateAsync(ApiKey apiKey);

        Task DeleteAsync(string id);
    }
}
