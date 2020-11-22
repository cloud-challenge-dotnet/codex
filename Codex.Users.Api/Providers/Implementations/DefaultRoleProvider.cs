﻿using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using System.Collections.Generic;

namespace Codex.Users.Api.Providers.Implementations
{
    public class DefaultRoleProvider : IRoleProvider
    {
        public List<Role> GetRoles()
        {
            return new()
            {
                new (Code: RoleConstant.ADMIN, UpperRoleCode: null),

                new(Code: RoleConstant.TENANT_MANAGER, UpperRoleCode: RoleConstant.ADMIN),

                new(Code: RoleConstant.USER, UpperRoleCode: RoleConstant.ADMIN)
            };
        }
    }
}