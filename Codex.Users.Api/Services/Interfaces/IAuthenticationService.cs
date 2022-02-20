using Codex.Models.Users;
using System.Threading.Tasks;

namespace Codex.Users.Api.Services.Interfaces;

public interface IAuthenticationService
{
    Task<Auth> AuthenticateAsync(UserLogin userLogin);
}