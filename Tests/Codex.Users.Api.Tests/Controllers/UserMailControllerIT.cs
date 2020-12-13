using Codex.Users.Api.Controllers;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Codex.Users.Api.Services.Interfaces;
using Codex.Models.Users;
using Codex.Models.Roles;

namespace Codex.Users.Api.Tests
{
    public class UserMailControllerIT : IClassFixture<Fixture>
    {
        public UserMailControllerIT()
        {
        }

        [Fact]
        public async Task SendActivateUserMail()
        {
            string tenantId = "global";
            User user = new() { Id = "Id1", Login = "login" };
            var userMailService = new Mock<IUserMailService>();

            userMailService.Setup(x => x.SendActivateUserMailAsync(It.IsAny<string>(), It.IsAny<User>())).Returns(
                Task.FromResult((User?)user)
            );

            var userMailController = new UserMailController(
                userMailService.Object
            );

            userMailController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );
            var result = await userMailController.SendActivateUserMail(tenantId, user);

            var okresult = Assert.IsType<OkResult>(result);

            userMailService.Verify(x => x.SendActivateUserMailAsync(It.IsAny<string>(), It.IsAny<User>()), Times.Once);


            // TODO add authorization role
            /*var authorizeAttributes = userMailController.GetType().GetMethod(nameof(UserMailController.SendActivateUserMail))?.GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.ADMIN, authorizeAttribute.Roles);*/
        }
    }
}
