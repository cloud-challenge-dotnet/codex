using Codex.Models.Roles;
using System.Collections.Generic;

namespace Codex.Users.Api.Services.Interfaces
{
    public interface IRoleService
    {
        public List<Role> GetRoles();
    }
}
