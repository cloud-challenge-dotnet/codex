using Codex.Tests.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Codex.Users.Api.Services.Implementations;
using Codex.Models.Users;
using Dapr.Client;
using System.Threading;
using Codex.Core.Models;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Codex.Core.RazorHelpers.Interfaces;
using Codex.Core.Cache;
using Codex.Models.Tenants;
using Codex.Core.Interfaces;
using Codex.Users.Api.Models;
using MongoDB.Bson;

namespace Codex.Users.Api.Tests
{
    public class UserMailServiceIT : IClassFixture<Fixture>
    {
        public UserMailServiceIT()
        {
        }

        [Fact]
        public async Task SendActivateUserMail()
        {
            string tenantId = "global";
            var userId = ObjectId.GenerateNewId();
            User user = new() { Id = userId, Login = "login" };
            var logger = new Mock<ILogger<UserMailService>>();
            var daprClient = new Mock<DaprClient>();
            var userService = new Mock<IUserService>();
            var mailService = new Mock<IMailService>();
            var razorPartialToStringRenderer = new Mock<IRazorPartialToStringRenderer>();
            var tenantCacheService = new Mock<TenantCacheService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)user)
            );

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string>() { 
                    { ConfigConstant.BackOfficeUrl, "" },
                    { ConfigConstant.SenderEmail, "" }
                }
            ));

            razorPartialToStringRenderer.Setup(x => x.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<UserNameActivationModel>())).Returns(
                Task.FromResult(@"<html></html>")
            );

            tenantCacheService.Setup(x => x.GetCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant(id: "global", name: "instance global"))
            );

            var userMailService = new UserMailService(
                logger.Object,
                daprClient.Object,
                razorPartialToStringRenderer.Object,
                userService.Object,
                mailService.Object,
                tenantCacheService.Object
            );

            await userMailService.SendActivateUserMailAsync(tenantId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()));
            razorPartialToStringRenderer.Verify(x => x.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<UserNameActivationModel>()), Times.Once);
            tenantCacheService.Verify(x => x.GetCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendActivateUserMail_Not_Found_User()
        {
            string tenantId = "global";
            var userId = ObjectId.GenerateNewId();
            User user = new() { Id = userId, Login = "login" };
            var logger = new Mock<ILogger<UserMailService>>();
            var daprClient = new Mock<DaprClient>();
            var userService = new Mock<IUserService>();
            var mailService = new Mock<IMailService>();
            var razorPartialToStringRenderer = new Mock<IRazorPartialToStringRenderer>();
            var tenantCacheService = new Mock<TenantCacheService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)null)
            );

            var userMailService = new UserMailService(
                logger.Object,
                daprClient.Object,
                razorPartialToStringRenderer.Object,
                userService.Object,
                mailService.Object,
                tenantCacheService.Object
            );

            await userMailService.SendActivateUserMailAsync(tenantId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
            razorPartialToStringRenderer.Verify(x => x.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<UserNameActivationModel>()), Times.Never);
            tenantCacheService.Verify(x => x.GetCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Never);
        }
    }
}
