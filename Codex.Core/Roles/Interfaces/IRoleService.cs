using Codex.Models.Roles;
using System.Collections.Generic;

namespace Codex.Core.Roles.Interfaces;

public interface IRoleService
{
    public List<Role> GetRoles();
}