using Codex.Core.Roles.Implementations;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Tests.Framework;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Codex.Users.Api.Tests.Services;

public class RoleServiceIt : IClassFixture<Fixture>
{
    [Fact]
    public void GetRoles()
    {
        var roleProvider = new Mock<IRoleProvider>();

        roleProvider.Setup(s => s.GetRoles()).Returns(new List<Role>()
        {
            new(RoleConstant.Admin)
        });

        RoleService roleService = new(roleProvider.Object);

        var roles = roleService.GetRoles();

        roleProvider.Verify(v => v.GetRoles());

        Assert.NotNull(roles);
        Assert.Single(roles);
        Assert.Equal(RoleConstant.Admin, roles[0].Code);
    }
}