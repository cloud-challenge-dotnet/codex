using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using System.Collections.Generic;
using System.Linq;

namespace Codex.Core.Roles.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleProvider _roleProvider;

        public RoleService(IRoleProvider roleProvider)
        {
            _roleProvider = roleProvider;
        }

        public List<Role> GetRoles()
        {
            return _roleProvider.GetRoles().Select(r => r with { }).ToList();
        }
    }
}
