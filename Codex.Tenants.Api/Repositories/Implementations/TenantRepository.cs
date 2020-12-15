using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codex.Tenants.Framework;
using Codex.Models.Tenants;
using MongoDB.Driver.Linq;
using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Core.Extensions;

namespace Codex.Tenants.Api.Repositories.Implementations
{
    public class TenantRepository : MongoTemplate<Tenant>, ITenantRepository
    {
        public TenantRepository(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService) : base(mongoDbSettings, tenantAccessService)
        {
        }

        public async Task<List<Tenant>> FindAllAsync()
        {
            var repository = await GetRepositoryAsync();

            var query =
                from e in repository.AsQueryable()
                select e;

            return query.ToList();
        }

        public async Task<Tenant?> UpdateAsync(Tenant tenant)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<Tenant>.Update;
            var updates = new List<UpdateDefinition<Tenant>>
            {
                update.Set(GetMongoPropertyName(nameof(tenant.Name)), tenant.Name)
            };
            tenant.Properties?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(tenant.Properties)), tenant.Properties)));

            return await repository.FindOneAndUpdateAsync(
                Builders<Tenant>.Filter.Where(it => it.Id == tenant.Id),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<Tenant>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<Tenant?> UpdatePropertyAsync(string tenantId, string propertyKey, List<string> values)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<Tenant>.Update;
            var updateDef = update.Set(GetMongoPropertyName($"{nameof(Tenant.Properties)}.{propertyKey}"), values);

            return await repository.FindOneAndUpdateAsync(
                Builders<Tenant>.Filter.Where(it => it.Id == tenantId),
                updateDef,
                options: new FindOneAndUpdateOptions<Tenant>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<Tenant?> UpdatePropertiesAsync(string tenantId, TenantProperties tenantProperties)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<Tenant>.Update;
            var updates = new List<UpdateDefinition<Tenant>>();

            foreach(var tenantProperty in tenantProperties)
            {
                updates.Add(update.Set(GetMongoPropertyName($"{nameof(Tenant.Properties)}.{tenantProperty.Key}"), tenantProperty.Value));
            }

            return await repository.FindOneAndUpdateAsync(
                Builders<Tenant>.Filter.Where(it => it.Id == tenantId),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<Tenant>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<Tenant?> DeletePropertyAsync(string tenantId, string propertyKey)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<Tenant>.Update;
            var updateDef = update.Unset($"{nameof(Tenant.Properties)}.{propertyKey}");

            return await repository.FindOneAndUpdateAsync(
                Builders<Tenant>.Filter.Where(it => it.Id == tenantId),
                updateDef,
                options: new FindOneAndUpdateOptions<Tenant>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }
    }
}
