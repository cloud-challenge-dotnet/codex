using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.Roles.Implementations;

[ExcludeFromCodeCoverage]
public class DefaultRoleProvider : IRoleProvider
{
    public List<Role> GetRoles()
    {
        return new()
        {
            new(Code: RoleConstant.Admin, UpperRoleCode: null),

            new(Code: RoleConstant.TenantManager, UpperRoleCode: RoleConstant.Admin),

            new(Code: RoleConstant.User, UpperRoleCode: RoleConstant.TenantManager)
        };
    }
}