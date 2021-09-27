using Codex.Core.Cache;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.RazorHelpers.Interfaces;
using Codex.Models.Tenants;
using Codex.Models.Users;
using Codex.Tenants.Framework.Resources;
using Codex.Tests.Framework;
using Codex.Users.Api.Models;
using Codex.Users.Api.Services.Implementations;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Users.Api.Tests
{
    public class UserMailServiceIT : IClassFixture<Fixture>
    {
        private readonly StringLocalizer<TenantFrameworkResource> _tenantFrameworkSl;

        public UserMailServiceIT()
        {
            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            _tenantFrameworkSl = new StringLocalizer<TenantFrameworkResource>(factory);
        }

        [Fact]
        public async Task SendActivateUserMail()
        {
            string tenantId = "global";
            var userId = ObjectId.GenerateNewId().ToString();
            User user = new() { Id = userId, Login = "login" };
            var logger = new Mock<ILogger<UserMailService>>();
            var daprClient = new Mock<DaprClient>();
            var userService = new Mock<IUserService>();
            var mailService = new Mock<IMailService>();
            var razorPartialToStringRenderer = new Mock<IRazorPartialToStringRenderer>();
            var tenantCacheService = new Mock<TenantCacheService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)user)
            );

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() {
                    { ConfigConstant.BackOfficeUrl, "" },
                    { ConfigConstant.SenderEmail, "" }
                }
            ));

            razorPartialToStringRenderer.Setup(x => x.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<UserNameActivationModel>())).Returns(
                Task.FromResult(@"<html></html>")
            );

            tenantCacheService.Setup(x => x.GetCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>())).Returns(
                Task.FromResult((Tenant?)new Tenant(Id: "global", Name: "instance global"))
            );

            var userMailService = new UserMailService(
                logger.Object,
                _tenantFrameworkSl,
                daprClient.Object,
                razorPartialToStringRenderer.Object,
                userService.Object,
                mailService.Object,
                tenantCacheService.Object
            );

            await userMailService.SendActivateUserMailAsync(tenantId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()));
            razorPartialToStringRenderer.Verify(x => x.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<UserNameActivationModel>()), Times.Once);
            tenantCacheService.Verify(x => x.GetCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendActivateUserMail_Not_Found_User()
        {
            string tenantId = "global";
            var userId = ObjectId.GenerateNewId().ToString();
            User user = new() { Id = userId, Login = "login" };
            var logger = new Mock<ILogger<UserMailService>>();
            var daprClient = new Mock<DaprClient>();
            var userService = new Mock<IUserService>();
            var mailService = new Mock<IMailService>();
            var razorPartialToStringRenderer = new Mock<IRazorPartialToStringRenderer>();
            var tenantCacheService = new Mock<TenantCacheService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)null)
            );

            var userMailService = new UserMailService(
                logger.Object,
                _tenantFrameworkSl,
                daprClient.Object,
                razorPartialToStringRenderer.Object,
                userService.Object,
                mailService.Object,
                tenantCacheService.Object
            );

            await userMailService.SendActivateUserMailAsync(tenantId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
            razorPartialToStringRenderer.Verify(x => x.RenderPartialToStringAsync(It.IsAny<string>(), It.IsAny<UserNameActivationModel>()), Times.Never);
            tenantCacheService.Verify(x => x.GetCacheAsync(It.IsAny<DaprClient>(), It.IsAny<string>()), Times.Never);
        }
    }
}
