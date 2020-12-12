using Codex.Models.Tenants;
using Codex.Models.Users;

namespace Codex.Users.Api.Models
{
    public record UserNameActivationModel(Tenant Tenant, User User, string ActivationLink);
}
