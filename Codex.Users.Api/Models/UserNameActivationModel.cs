using Codex.Models.Tenants;
using Codex.Models.Users;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Users.Api.Models
{
    [ExcludeFromCodeCoverage]
    public record UserNameActivationModel(Tenant Tenant, User User, string ActivationLink);
}
