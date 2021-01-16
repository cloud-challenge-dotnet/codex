using Codex.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> FindAllAsync(UserCriteria userCriteria);

        Task<User?> FindOneAsync(string id);

        Task<User> CreateAsync(string tenantId, UserCreator userCreator);

        Task<User?> UpdateAsync(User user);

        Task<User?> UpdatePasswordAsync(string userId, string password);

        Task<User?> ActivateUserAsync(User user, string activationCode);
    }
}
