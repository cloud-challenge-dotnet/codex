using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codex.Tenants.Framework;
using MongoDB.Driver.Linq;
using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Core.Extensions;
using Codex.Tenants.Api.Repositories.Models;

namespace Codex.Tenants.Api.Repositories.Implementations
{
    public class TenantRepository : MongoTemplate<TenantRow, string>, ITenantRepository
    {
        public TenantRepository(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService) : base(mongoDbSettings, tenantAccessService)
        {
        }

        public async Task<List<TenantRow>> FindAllAsync()
        {
            var repository = await GetRepositoryAsync();

            var query =
                from e in repository.AsQueryable()
                select e;

            return query.ToList();
        }

        public async Task<TenantRow?> UpdateAsync(TenantRow tenant)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<TenantRow>.Update;
            var updates = new List<UpdateDefinition<TenantRow>>
            {
                update.Set(GetMongoPropertyName(nameof(tenant.Name)), tenant.Name)
            };
            tenant.Properties?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(tenant.Properties)), tenant.Properties)));

            return await repository.FindOneAndUpdateAsync(
                Builders<TenantRow>.Filter.Where(it => it.Id == tenant.Id),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<TenantRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<TenantRow?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<TenantRow>.Update;
            var updateDef = update.Set(GetMongoPropertyName($"{nameof(TenantRow.Properties)}.{propertyKey}"), values);

            return await repository.FindOneAndUpdateAsync(
                Builders<TenantRow>.Filter.Where(it => it.Id == tenantId),
                updateDef,
                options: new FindOneAndUpdateOptions<TenantRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<TenantRow?> UpdatePropertiesAsync(string tenantId, Dictionary<string, List<string>> tenantProperties)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<TenantRow>.Update;
            var updates = new List<UpdateDefinition<TenantRow>>();

            foreach(var tenantProperty in tenantProperties)
            {
                updates.Add(update.Set(GetMongoPropertyName($"{nameof(TenantRow.Properties)}.{tenantProperty.Key}"), tenantProperty.Value));
            }

            return await repository.FindOneAndUpdateAsync(
                Builders<TenantRow>.Filter.Where(it => it.Id == tenantId),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<TenantRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<TenantRow?> DeletePropertyAsync(string tenantId, string propertyKey)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<TenantRow>.Update;
            var updateDef = update.Unset($"{nameof(TenantRow.Properties)}.{propertyKey}");

            return await repository.FindOneAndUpdateAsync(
                Builders<TenantRow>.Filter.Where(it => it.Id == tenantId),
                updateDef,
                options: new FindOneAndUpdateOptions<TenantRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }
    }
}
