using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Controllers;
using Codex.Users.Api.Resources;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Users.Api.Tests.Controllers
{
    public class AuthenticationControllerIT : IClassFixture<Fixture>
    {
        private readonly IStringLocalizer<UserResource> _stringLocalizer;

        public AuthenticationControllerIT()
        {
            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            _stringLocalizer = new StringLocalizer<UserResource>(factory);
        }

        [Fact]
        public async Task Authenticate()
        {
            string tenantId = "global";
            var userLogin = new UserLogin() { TenantId = tenantId };
            var authenticationService = new Mock<IAuthenticationService>();

            authenticationService.Setup(s => s.AuthenticateAsync(It.IsAny<UserLogin>()))
                .Returns(Task.FromResult(new Auth(Id: "ID1", Login: "Login", Token: "5634534564")));

            AuthenticationController authenticationController = new(authenticationService.Object, _stringLocalizer);

            var result = await authenticationController.Authenticate(tenantId, userLogin);

            authenticationService.Verify(v => v.AuthenticateAsync(It.IsAny<UserLogin>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var auth = Assert.IsType<Auth>(objectResult.Value);

            Assert.NotNull(auth);
            Assert.Equal("ID1", auth.Id);
            Assert.Equal("Login", auth.Login);
            Assert.Equal("5634534564", auth.Token);
        }

        [Fact]
        public async Task Authenticate_Invalid_Tenant_Id()
        {
            string tenantId = "global";
            var userLogin = new UserLogin() { TenantId = tenantId };
            var authenticationService = new Mock<IAuthenticationService>();

            authenticationService.Setup(s => s.AuthenticateAsync(It.IsAny<UserLogin>()))
                .Returns(Task.FromResult(new Auth(Id: "ID1", Login: "Login", Token: "5634534564")));

            AuthenticationController authenticationController = new(authenticationService.Object, _stringLocalizer);

            await Assert.ThrowsAsync<ArgumentException>(() => authenticationController.Authenticate("demo", userLogin));

            authenticationService.Verify(v => v.AuthenticateAsync(It.IsAny<UserLogin>()), Times.Never);
        }
    }
}
