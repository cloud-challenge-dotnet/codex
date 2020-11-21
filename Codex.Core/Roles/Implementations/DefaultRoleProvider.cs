using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using System.Collections.Generic;

namespace Codex.Core.Roles.Implementations
{
    public class DefaultRoleProvider : IRoleProvider
    {
        public List<Role> GetRoles()
        {
            return new()
            {
                new (Code: RoleConstant.ADMIN, ParentRoleCode: null),

                new(Code: RoleConstant.TENANT_MANAGER, ParentRoleCode: RoleConstant.ADMIN)
            };
        }
    }
}
