using Codex.Core.Interfaces;
using Codex.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<User>> FindAllAsync(UserCriteria userCriteria);

        Task<User?> UpdateAsync(User tenant);
    }
}
