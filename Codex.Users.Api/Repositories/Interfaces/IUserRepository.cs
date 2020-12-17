using Codex.Core.Interfaces;
using Codex.Models.Users;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User, ObjectId>
    {
        Task<List<User>> FindAllAsync(UserCriteria userCriteria);

        Task<User?> UpdateAsync(User user);

        Task<User?> UpdateActivationCodeAsync(ObjectId userId, string activationCode);
    }
}
