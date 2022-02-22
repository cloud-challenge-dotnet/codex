using Codex.Core.Models;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Interfaces;
using Codex.Security.Api.Repositories.Models;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Resources;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Security.Api.Repositories.Implementations;

public class ApiKeyRepository : MongoTemplate<ApiKeyRow, string>, IApiKeyRepository
{
    public ApiKeyRepository(MongoDbSettings mongoDbSettings,
        ITenantAccessService tenantAccessService,
        IStringLocalizer<TenantFrameworkResource> sl) : base(mongoDbSettings, tenantAccessService, sl)
    {
    }

    public async Task<List<ApiKeyRow>> FindAllAsync(ApiKeyCriteria apiKeyCriteria)
    {
        var repository = await GetRepositoryAsync();

        var query = repository.AsQueryable();

        return query.ToList();
    }

    public async Task<ApiKeyRow?> UpdateAsync(ApiKeyRow apiKey)
    {
        var repository = await GetRepositoryAsync();

        var update = Builders<ApiKeyRow>.Update;
        var updates = new List<UpdateDefinition<ApiKeyRow>>
        {
            update.Set(GetMongoPropertyName(nameof(apiKey.Roles)), apiKey.Roles)
        };

        foreach (var KeyValuePair in apiKey.Name)
        {
            updates.Add(update.Set(GetMongoPropertyName($"{nameof(ApiKeyRow.Name)}.{KeyValuePair.Key}"), KeyValuePair.Value));
        }

        return await repository.FindOneAndUpdateAsync(
            Builders<ApiKeyRow>.Filter.Where(it => it.Id == apiKey.Id),
            update.Combine(updates),
            options: new FindOneAndUpdateOptions<ApiKeyRow>
            {
                ReturnDocument = ReturnDocument.After
            }
        );
    }
}