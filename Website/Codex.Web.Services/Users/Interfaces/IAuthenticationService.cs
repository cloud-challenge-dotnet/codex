using Codex.Models.Users;
using System.Threading.Tasks;

namespace Codex.Web.Services.Users.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Auth> AuthenticateAsync(UserLogin userLogin);

        Task ClearAuthenticationAsync();
    }
}
