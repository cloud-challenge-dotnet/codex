using Codex.Core.Models;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Interfaces;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Interfaces;

namespace Codex.Security.Api.Repositories.Implementations
{
    public class ApiKeyRepository : MongoTemplate<ApiKey, string>, IApiKeyRepository
    {
        public ApiKeyRepository(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService) : base(mongoDbSettings, tenantAccessService)
        {
        }

        public async Task<List<ApiKey>> FindAllAsync(ApiKeyCriteria apiKeyCriteria)
        {
            var repository = await GetRepositoryAsync();

            var query = repository.AsQueryable();

            return query.ToList();
        }

        public async Task<ApiKey?> UpdateAsync(ApiKey apiKey)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<ApiKey>.Update;
            var updates = new List<UpdateDefinition<ApiKey>>
            {
                update.Set(GetMongoPropertyName(nameof(apiKey.Name)), apiKey.Name),
                update.Set(GetMongoPropertyName(nameof(apiKey.Roles)), apiKey.Roles)
            };

            return await repository.FindOneAndUpdateAsync(
                Builders<ApiKey>.Filter.Where(it => it.Id == apiKey.Id),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<ApiKey>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }
    }
}

