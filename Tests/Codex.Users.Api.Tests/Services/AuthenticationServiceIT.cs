using Codex.Core.Cache;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Models.Users;
using Codex.Tenants.Framework.Resources;
using Codex.Tests.Framework;
using Codex.Users.Api.Exceptions;
using Codex.Users.Api.Resources;
using Codex.Users.Api.Services.Implementations;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Dapr.Client.Http;
using Microsoft.Extensions.Configuration;
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

namespace Codex.Users.Api.Tests.Services
{
    public class AuthenticationServiceIT : IClassFixture<Fixture>
    {
        private readonly IStringLocalizer<UserResource> _stringLocalizer;

        private readonly IStringLocalizer<TenantFrameworkResource> _tenantFrameworkSl;

        public AuthenticationServiceIT()
        {
            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            _stringLocalizer = new StringLocalizer<UserResource>(factory);
            _tenantFrameworkSl = new StringLocalizer<TenantFrameworkResource>(factory);
        }

        [Fact]
        public async Task AuthenticateAsync()
        {
            var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");
            var userId = ObjectId.GenerateNewId().ToString();

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("global", "", null)));

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
                .Returns(Task.FromResult(new List<User>(){
                    new () { Id= userId, Login = "Login", PasswordHash="123123313" , Active = true, Roles = new(){ RoleConstant.ADMIN } }
                })
            );

            roleService.Setup(x => x.GetRoles())
                .Returns(new List<Role>(){
                    new (RoleConstant.ADMIN, UpperRoleCode: null),
                    new (RoleConstant.USER, UpperRoleCode: RoleConstant.ADMIN)
                });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string>() { { ConfigConstant.PasswordSalt, "" } }
            ));

            passwordHasher.Setup(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("123123313");

            //jwtToken
            configurationSection.Setup(c => c.Value).Returns("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e");

            configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);

            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var auth = await authenticationService.AuthenticateAsync(userLogin);

            tenantCacheService.Verify(x => x.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
            userService.Verify(x => x.FindAllAsync(It.IsAny<UserCriteria>()), Times.Once);
            roleService.Verify(x => x.GetRoles(), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
            daprClient.Verify(x => x.InvokeMethodAsync<List<User>>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()), Times.Never);
            passwordHasher.Verify(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.NotNull(auth);
            Assert.Equal(userId.ToString(), auth.Id);
            Assert.NotNull(auth.Token);
        }

        [Fact]
        public async Task AuthenticateAsync_Empty_Login()
        {
            var userLogin = new UserLogin(Login: "", Password: "test", TenantId: "global");

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await authenticationService.AuthenticateAsync(userLogin)
            );

            Assert.Equal("INVALID_LOGIN", exception.Code);
        }

        [Fact]
        public async Task AuthenticateAsync_Empty_Password()
        {
            var userLogin = new UserLogin(Login: "Login", Password: "", TenantId: "global");

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await authenticationService.AuthenticateAsync(userLogin)
            );

            Assert.Equal("INVALID_LOGIN", exception.Code);
        }

        [Fact]
        public async Task AuthenticateAsync_User_Not_Found()
        {
            var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("global", "", null)));

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
                .Returns(Task.FromResult(new List<User>() { })
            );

            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await authenticationService.AuthenticateAsync(userLogin)
            );

            Assert.Equal("INVALID_LOGIN", exception.Code);
        }

        [Fact]
        public async Task AuthenticateAsync_User_Invalid_Password()
        {
            var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");
            var userId = ObjectId.GenerateNewId().ToString();

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("global", "", null)));

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
                .Returns(Task.FromResult(new List<User>(){
                    new () { Id= userId, PasswordHash="123123313" , Active = true, Roles = new(){ RoleConstant.ADMIN } }
                })
            );

            roleService.Setup(x => x.GetRoles())
                .Returns(new List<Role>(){
                    new (RoleConstant.ADMIN, UpperRoleCode: null),
                    new (RoleConstant.USER, UpperRoleCode: RoleConstant.ADMIN)
                });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string>() { { ConfigConstant.PasswordSalt, "" } }
            ));

            passwordHasher.Setup(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("447899898");

            //jwtToken
            configurationSection.Setup(c => c.Value).Returns("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e");

            configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);


            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
                async () => await authenticationService.AuthenticateAsync(userLogin)
            );

            Assert.Equal("INVALID_LOGIN", exception.Code);
        }

        [Fact]
        public async Task AuthenticateAsync_User_Disabled()
        {
            var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");
            var userId = ObjectId.GenerateNewId().ToString();

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("global", "", null)));

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
                .Returns(Task.FromResult(new List<User>() {
                    new () { Id= userId, Login= "Login", PasswordHash="123123313" , Active = false, Roles = new(){ RoleConstant.ADMIN } }
                })
            );

            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var exception = await Assert.ThrowsAsync<DisabledUserException>(
                async () => await authenticationService.AuthenticateAsync(userLogin)
            );

            Assert.Equal("DISABLED_USER", exception.Code);
        }

        [Fact]
        public async Task AuthenticateAsync_With_Global_Tenant_User()
        {
            var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "demo");
            var userId = ObjectId.GenerateNewId().ToString();

            var logger = new Mock<ILogger<AuthenticationService>>();
            var daprClient = new Mock<DaprClient>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var userService = new Mock<IUserService>();
            var configuration = new Mock<IConfiguration>();
            var configurationSection = new Mock<IConfigurationSection>();
            var roleService = new Mock<IRoleService>();
            var tenantCacheService = new Mock<TenantCacheService>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("demo", "", null)));

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
                .Returns(Task.FromResult(new List<User>() { })
            );

            roleService.Setup(x => x.GetRoles())
                .Returns(new List<Role>(){
                    new (RoleConstant.ADMIN, UpperRoleCode: null),
                    new (RoleConstant.USER, UpperRoleCode: RoleConstant.ADMIN)
                });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string>() {
                    { ConfigConstant.PasswordSalt, "" },
                    { ConfigConstant.MicroserviceApiKey, "" }
                }
            ));

            daprClient.Setup(x => x.InvokeMethodAsync<List<User>>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<List<User>>(new List<User>() {
                    new User(){Id = userId, Login = "Login", PasswordHash = "123123313" }
                }));

            passwordHasher.Setup(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("123123313");

            //jwtToken
            configurationSection.Setup(c => c.Value).Returns("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e");

            configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);

            AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
                configuration.Object, roleService.Object, tenantCacheService.Object, _stringLocalizer, _tenantFrameworkSl);

            var auth = await authenticationService.AuthenticateAsync(userLogin);

            tenantCacheService.Verify(x => x.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
            userService.Verify(x => x.FindAllAsync(It.IsAny<UserCriteria>()), Times.Once);
            roleService.Verify(x => x.GetRoles(), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            daprClient.Verify(x => x.InvokeMethodAsync<List<User>>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()), Times.Once);
            passwordHasher.Verify(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.NotNull(auth);
            Assert.Equal(userId.ToString(), auth.Id);
            Assert.NotNull(auth.Token);
        }
    }
}
