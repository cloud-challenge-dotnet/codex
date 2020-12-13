using Codex.Models.Users;
using System.Threading.Tasks;

namespace Codex.Users.Api.Services.Interfaces
{
    public interface IUserMailService
    {
        public Task SendActivateUserMailAsync(string tenantId, User user);
    }
}
