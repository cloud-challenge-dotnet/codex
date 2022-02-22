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

namespace Codex.Users.Api.Repositories.Implementations;

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
        user.FirstName?.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.FirstName)), it)));
        user.LastName?.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.LastName)), it)));
        user.PhoneNumber?.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.PhoneNumber)), it)));
        user.Roles.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.Roles)), it)));
        user.LanguageCultureName.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.LanguageCultureName)), it)));
        user.PasswordHash?.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.PasswordHash)), it)));
        user.ActivationCode?.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.ActivationCode)), it)));
        user.ActivationValidity?.Also(it => updates.Add(update.Set(GetMongoPropertyName(nameof(user.ActivationValidity)), it)));

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