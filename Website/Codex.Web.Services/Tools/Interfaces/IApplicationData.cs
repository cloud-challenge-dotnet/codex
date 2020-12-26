using Codex.Models.Users;
using System.Threading.Tasks;

namespace Codex.Web.Services.Tools.Interfaces
{
    public interface IApplicationData
    {
        Auth? Auth { get; }

        Task SetAuthAsync(Auth? value);

        string? TenantId { get; }

        Task SetTenantIdAsync(string? value);

        Task InitializeAsync();
    }
}
