using System.Threading.Tasks;
using Codex.Models.Roles;
using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Controllers;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace Codex.Users.Api.Tests.Controllers;

public class UserMailControllerIt : IClassFixture<Fixture>
{
    [Fact]
    public async Task SendActivateUserMail()
    {
        string tenantId = "global";
        var userId = ObjectId.GenerateNewId().ToString();
        User user = new() { Id = userId, Login = "login" };
        var userMailService = new Mock<IUserMailService>();

        userMailService.Setup(x => x.SendActivateUserMailAsync(It.IsAny<string>(), It.IsAny<User>())).Returns(
            Task.FromResult((User?)user)
        );

        var userMailController = new UserMailController(
            userMailService.Object
        );

        userMailController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
            tenantId: "global",
            userId: userId,
            userName: "login",
            roles: new() { RoleConstant.TenantManager }
        );
        var result = await userMailController.SendActivateUserMail(tenantId, user);

        Assert.IsType<OkResult>(result);

        userMailService.Verify(x => x.SendActivateUserMailAsync(It.IsAny<string>(), It.IsAny<User>()), Times.Once);
    }
}