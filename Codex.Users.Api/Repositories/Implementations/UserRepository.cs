﻿using Codex.Core.Models;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Interfaces;
using Codex.Models.Users;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Core.Extensions;

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
            var updates = new List<UpdateDefinition<User>>
            {
                update.Set(GetMongoPropertyName(nameof(user.Login)), user.Login),
                update.Set(GetMongoPropertyName(nameof(user.Email)), user.Email)
            };
            user.FirstName?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.FirstName)), user.FirstName)));
            user.LastName?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.LastName)), user.LastName)));
            user.PhoneNumber?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.PhoneNumber)), user.PhoneNumber)));
            user.Roles?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.Roles)), user.Roles)));
            user.PasswordHash?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.PasswordHash)), user.PasswordHash)));
            updates.Add(update.Set(GetMongoPropertyName(nameof(user.EmailConfirmed)), user.EmailConfirmed));
            updates.Add(update.Set(GetMongoPropertyName(nameof(user.PhoneConfirmed)), user.PhoneConfirmed));
            updates.Add(update.Set(GetMongoPropertyName(nameof(user.Active)), user.Active));

            return await repository.FindOneAndUpdateAsync(
                Builders<User>.Filter.Where(it => it.Id == user.Id),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<User>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }
    }
}

