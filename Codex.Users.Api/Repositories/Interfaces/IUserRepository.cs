using Codex.Core.Interfaces;
using Codex.Models.Users;
using Codex.Users.Api.Repositories.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Repositories.Interfaces;

public interface IUserRepository : IRepository<UserRow, ObjectId>
{
    Task<List<UserRow>> FindAllAsync(UserCriteria userCriteria);

    Task<UserRow?> UpdateAsync(UserRow user);

    Task<UserRow?> UpdatePasswordAsync(ObjectId userId, string passwordHash);

    Task<UserRow?> UpdateActivationCodeAsync(ObjectId userId, string activationCode);
}