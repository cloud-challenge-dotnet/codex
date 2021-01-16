using Codex.Models.Users;
using System.Threading.Tasks;

namespace Codex.Web.Services.Users.Interfaces
{
    public interface IUserService
    {
        Task<User> FindOneAsync(string userId);

        Task<User> CreateAsync(UserCreator userCreator);

        Task<User> UpdateAsync(User user);

        Task<User> UpdatePasswordAsync(string userId, string password);
    }
}
