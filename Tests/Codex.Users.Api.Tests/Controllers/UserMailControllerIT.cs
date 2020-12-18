using Codex.Users.Api.Controllers;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Codex.Users.Api.Services.Interfaces;
using Codex.Models.Users;
using Codex.Models.Roles;
using MongoDB.Bson;

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
            var userId = ObjectId.GenerateNewId();
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
                userId: userId.ToString(),
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );
            var result = await userMailController.SendActivateUserMail(tenantId, user);

            var okresult = Assert.IsType<OkResult>(result);

            userMailService.Verify(x => x.SendActivateUserMailAsync(It.IsAny<string>(), It.IsAny<User>()), Times.Once);


            // TODO add authorization role
            /*var authorizeAttributes = userMailController.GetType().GetMethod(nameof(UserMailController.SendActivateUserMail))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);
            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.ADMIN, authorizeAttribute.Roles);*/
        }
    }
}
