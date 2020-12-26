using Codex.Models.Users;
using Codex.Web.Services.Tools.Interfaces;

namespace Codex.Web.Services.Tools.Implementations
{
    public class ApplicationData: IApplicationData
    {
        public Auth? Auth { get; set; }

        public string? TenantId { get; set; }
    }
}
