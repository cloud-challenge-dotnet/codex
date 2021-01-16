using Codex.Core.Extensions;
using Codex.Core.Models;
using Codex.Models.Users;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Resources;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Repositories.Models;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Repositories.Implementations
{
    public class UserRepository : MongoTemplate<UserRow, ObjectId>, IUserRepository
    {
        public UserRepository(MongoDbSettings mongoDbSettings,
            ITenantAccessService tenantAccessService,
            IStringLocalizer<TenantFrameworkResource> sl) : base(mongoDbSettings, tenantAccessService, sl)
        {
        }

        public async Task<List<UserRow>> FindAllAsync(UserCriteria userCriteria)
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

        public override async Task<UserRow> InsertAsync(UserRow document)
        {
            document = document with
            {
                CreationDate = DateTime.Now,
                ModificationDate = DateTime.Now
            };

            return await base.InsertAsync(document);
        }

        public async Task<UserRow?> UpdateAsync(UserRow user)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<UserRow>.Update;
            var updates = new List<UpdateDefinition<UserRow>>
            {
                update.Set(GetMongoPropertyName(nameof(user.Login)), user.Login),
                update.Set(GetMongoPropertyName(nameof(user.Email)), user.Email),
                update.Set(GetMongoPropertyName(nameof(user.Active)), user.Active),
                update.SetOnInsert(GetMongoPropertyName(nameof(user.CreationDate)), DateTime.Now),
                update.Set(GetMongoPropertyName(nameof(user.ModificationDate)), DateTime.Now)
            };
            user.FirstName?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.FirstName)), user.FirstName)));
            user.LastName?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.LastName)), user.LastName)));
            user.PhoneNumber?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.PhoneNumber)), user.PhoneNumber)));
            user.Roles?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.Roles)), user.Roles)));
            user.LanguageCultureName?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.LanguageCultureName)), user.LanguageCultureName)));
            user.PasswordHash?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.PasswordHash)), user.PasswordHash)));
            user.ActivationCode?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.ActivationCode)), user.ActivationCode)));
            user.ActivationValidity?.Also(x => updates.Add(update.Set(GetMongoPropertyName(nameof(user.ActivationValidity)), user.ActivationValidity)));

            return await repository.FindOneAndUpdateAsync(
                Builders<UserRow>.Filter.Where(it => it.Id == user.Id),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<UserRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<UserRow?> UpdatePasswordAsync(ObjectId userId, string passwordHash)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<UserRow>.Update;
            var updates = new List<UpdateDefinition<UserRow>>
            {
                update.Set(GetMongoPropertyName(nameof(UserRow.PasswordHash)), passwordHash),
                update.Set(GetMongoPropertyName(nameof(UserRow.ModificationDate)), DateTime.Now)
            };
            return await repository.FindOneAndUpdateAsync(
                Builders<UserRow>.Filter.Where(it => it.Id == userId),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<UserRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }

        public async Task<UserRow?> UpdateActivationCodeAsync(ObjectId userId, string activationCode)
        {
            var repository = await GetRepositoryAsync();

            var update = Builders<UserRow>.Update;
            var updates = new List<UpdateDefinition<UserRow>>
            {
                update.Set(GetMongoPropertyName(nameof(UserRow.ModificationDate)), DateTime.Now),
                update.Set(GetMongoPropertyName(nameof(UserRow.ActivationCode)), activationCode),
                update.Set(GetMongoPropertyName(nameof(UserRow.ActivationValidity)), DateTime.Now.AddMinutes(15))
            };

            return await repository.FindOneAndUpdateAsync(
                Builders<UserRow>.Filter.Where(it => it.Id == userId),
                update.Combine(updates),
                options: new FindOneAndUpdateOptions<UserRow>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );
        }
    }
}

