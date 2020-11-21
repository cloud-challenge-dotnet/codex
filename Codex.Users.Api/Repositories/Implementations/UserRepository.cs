using Codex.Core.Models;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Interfaces;
using Codex.Models.Users;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Repositories.Interfaces
{
    public class UserRepository : MongoTemplate<User>, IUserRepository
    {
        public UserRepository(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService) : base(mongoDbSettings, tenantAccessService)
        {
        }

        public async Task<List<User>> FindAllAsync(UserCriteria userCriteria)
        {
            var repository = await GetRepositoryAsync();

            var query = repository.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userCriteria.Login))
            {
                query = query.Where(u => u.Login.ToLowerInvariant() == userCriteria.Login.ToLowerInvariant());
            }

            if (!string.IsNullOrWhiteSpace(userCriteria.Email))
            {
                query = query.Where(u => u.Email.ToLowerInvariant() == userCriteria.Email.ToLowerInvariant());
            }

            return query.ToList();
        }

        public async Task<User?> UpdateAsync(User user)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<User>.Update;
            var updateDef = update.Set(GetMongoPropertyName(nameof(user.Login)), user.Login);

            return await repository.FindOneAndUpdateAsync(
                Builders<User>.Filter.Where(it => it.Id == user.Id),
                updateDef,
                options: new FindOneAndUpdateOptions<User>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }
    }
}

