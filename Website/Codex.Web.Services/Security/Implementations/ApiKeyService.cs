using Codex.Models.Security;
using Codex.Web.Services.Security.Interfaces;
using Codex.Web.Services.Tools.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Web.Services.Users.Implementations
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IHttpManager _httpManager;

        public ApiKeyService(
            IHttpManager httpManager
        )
        {
            _httpManager = httpManager;
        }

        public Task<ApiKey> FindOneAsync(string id)
        {
            return _httpManager.GetAsync<ApiKey>($"securityApi/ApiKey/{id}");
        }

        public Task<List<ApiKey>> FindAllAsync()
        {
            return _httpManager.GetAsync<List<ApiKey>>($"securityApi/ApiKey");
        }

        public Task<ApiKey> CreateAsync(ApiKey apiKey)
        {
            return _httpManager.PostAsync<ApiKey>("securityApi/ApiKey", apiKey);
        }

        public Task<ApiKey> UpdateAsync(ApiKey apiKey)
        {
            return _httpManager.PutAsync<ApiKey>($"securityApi/ApiKey/{apiKey.Id}", apiKey);
        }

        public Task DeleteAsync(string id)
        {
            return _httpManager.DeleteAsync($"securityApi/ApiKey/{id}");
        }
    }
}
