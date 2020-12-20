using Codex.Models.Users;

namespace Codex.Web.Services.Tools.Interfaces
{
    public interface IApplicationData
    {
        Auth? Auth { get; set; }

        string? TenantId { get; set; }
    }
}
