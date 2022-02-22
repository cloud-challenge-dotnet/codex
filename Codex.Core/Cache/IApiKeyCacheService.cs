using System.Threading.Tasks;
using Codex.Models.Security;

namespace Codex.Core.Cache;

public interface IApiKeyCacheService : ICacheService<ApiKey>
{
    Task<ApiKey> GetApiKeyAsync(string providedApiKey, string tenantId);
}