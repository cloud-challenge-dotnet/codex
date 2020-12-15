using Codex.Core.Roles.Implementations;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Tests.Framework;
using Codex.Users.Api.Services.Implementations;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Codex.Users.Api.Tests.Services
{
    public class RoleServiceIT : IClassFixture<Fixture>
    {
        public RoleServiceIT()
        {
        }

        [Fact]
        public void GetRoles()
        {
            var roleProvider = new Mock<IRoleProvider>();

            roleProvider.Setup(s => s.GetRoles()).Returns(new List<Role>()
            {
                new(RoleConstant.ADMIN, null)
            });

            RoleService roleService = new(roleProvider.Object);

            var roles = roleService.GetRoles();

            roleProvider.Verify(v => v.GetRoles());

            Assert.NotNull(roles);
            Assert.Single(roles);
            Assert.Equal(RoleConstant.ADMIN, roles![0].Code);
        }
    }
}
