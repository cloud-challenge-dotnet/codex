using Codex.Models.Users;
using System.Threading.Tasks;

namespace Codex.Web.Services.Users.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateAsync(UserCreator userCreator);
    }
}
