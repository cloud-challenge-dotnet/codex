using Codex.Core.ApiKeys;
using Codex.Core.ApiKeys.Models;
using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Models.Security;
using Codex.Tests.Framework;
using Dapr.Client;
using Dapr.Client.Http;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Core.Tests.ApiKeys
{
    public class ApiKeyAuthenticationHandlerIT : IClassFixture<Fixture>
    {
        public ApiKeyAuthenticationHandlerIT()
        {
        }

        [Fact]
        public async Task Authenticate_Without_Api_Key()
        {
            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = new DefaultHttpContext();

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.False(authenticateResult.Succeeded);
            Assert.True(authenticateResult.None);
        }

        [Fact]
        public async Task Authenticate_With_Whitespace_Api_Key_In_Header_Content()
        {
            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues("global") },
                    { HttpHeaderConstant.ApiKey, new StringValues("       ") }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.False(authenticateResult.Succeeded);
            Assert.True(authenticateResult.None);
        }


        [Fact]
        public async Task Authenticate_With_Api_Key_Without_Tenant_Id()
        {
            string tenantId = "global";
            string apiKey = $"fgdfgkfdgmfmdkgmkdlgklmfdg";
            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: tenantId,
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues(tenantId) },
                    { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.False(authenticateResult.Succeeded);
            Assert.NotNull(authenticateResult.Failure);
            Assert.Equal("Invalid API Key provided.", authenticateResult.Failure!.Message);
        }

        [Fact]
        public async Task Authenticate_With_MicroserviceApiKey()
        {
            string tenantId = "global";
            string apiKey = $"{tenantId}.fgdfgkfdgmfmdkgmkdlgklmfdg";

            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() { 
                new Role(RoleConstant.USER, UpperRoleCode: RoleConstant.TENANT_MANAGER),
                new Role(RoleConstant.TENANT_MANAGER, UpperRoleCode: RoleConstant.ADMIN),
                new Role(RoleConstant.ADMIN)
            });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string> { { ConfigConstant.MicroserviceApiKey, "fgdfgkfdgmfmdkgmkdlgklmfdg" } }
            ));

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: tenantId,
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues("global") },
                    { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.True(authenticateResult.Succeeded);
            Assert.False(authenticateResult.None);
            Assert.NotNull(authenticateResult.Ticket);
            var authenticationTicket = Assert.IsType<AuthenticationTicket>(authenticateResult.Ticket);

            Assert.Equal(ApiKeyAuthenticationOptions.DefaultScheme, authenticationTicket.AuthenticationScheme);
            Assert.NotEmpty(authenticationTicket.Principal.Claims);

            var claims = authenticationTicket.Principal.Claims.Select(x => (x.Type, x.Value)).ToList();
            Assert.Contains((ClaimConstant.TenantId, tenantId), claims);
            Assert.Contains((ClaimTypes.Name, "Inter call Micro Services"), claims);
            Assert.Contains((ClaimTypes.NameIdentifier, "API_KEY"), claims);
        }

        [Fact]
        public async Task Authenticate_With_ApiKey()
        {
            string tenantId = "global";
            string apiKey = $"{tenantId}.ABCDEFGH";

            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
                new Role(RoleConstant.USER, UpperRoleCode: RoleConstant.TENANT_MANAGER),
                new Role(RoleConstant.TENANT_MANAGER, UpperRoleCode: RoleConstant.ADMIN),
                new Role(RoleConstant.ADMIN)
            });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string> { { ConfigConstant.MicroserviceApiKey, "fgdfgkfdgmfmdkgmkdlgklmfdg" } }
            ));

            daprClient.Setup(x => x.InvokeMethodAsync<ApiKey>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<ApiKey>(new ApiKey("test", "test api key", new List<string>() {
                    RoleConstant.USER
                })));

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: tenantId,
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues("global") },
                    { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.True(authenticateResult.Succeeded);
            Assert.False(authenticateResult.None);
            Assert.NotNull(authenticateResult.Ticket);
            var authenticationTicket = Assert.IsType<AuthenticationTicket>(authenticateResult.Ticket);

            Assert.Equal(ApiKeyAuthenticationOptions.DefaultScheme, authenticationTicket.AuthenticationScheme);
            Assert.NotEmpty(authenticationTicket.Principal.Claims);

            var claims = authenticationTicket.Principal.Claims.Select(x => (x.Type, x.Value)).ToList();
            Assert.Contains((ClaimConstant.TenantId, tenantId), claims);
            Assert.Contains((ClaimTypes.Name, "test api key"), claims);
            Assert.Contains((ClaimTypes.NameIdentifier, "API_KEY"), claims);
        }

        [Fact]
        public async Task Authenticate_With_UnknownApiKey()
        {
            string tenantId = "global";
            string apiKey = $"{tenantId}.ABCDECCDFSFDF";

            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
                new Role(RoleConstant.USER, UpperRoleCode: RoleConstant.TENANT_MANAGER),
                new Role(RoleConstant.TENANT_MANAGER, UpperRoleCode: RoleConstant.ADMIN),
                new Role(RoleConstant.ADMIN)
            });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string> { { ConfigConstant.MicroserviceApiKey, "fgdfgkfdgmfmdkgmkdlgklmfdg" } }
            ));

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: tenantId,
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues("global") },
                    { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.False(authenticateResult.Succeeded);
            Assert.NotNull(authenticateResult.Failure);
            Assert.Equal("Invalid API Key provided.", authenticateResult.Failure!.Message);
        }

        [Fact]
        public async Task Authenticate_With_ApiKey_RPC_Exception()
        {
            string tenantId = "global";
            string apiKey = $"{tenantId}.ABCDEFGH";

            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
                new Role(RoleConstant.USER, UpperRoleCode: RoleConstant.TENANT_MANAGER),
                new Role(RoleConstant.TENANT_MANAGER, UpperRoleCode: RoleConstant.ADMIN),
                new Role(RoleConstant.ADMIN)
            });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string> { { ConfigConstant.MicroserviceApiKey, "fgdfgkfdgmfmdkgmkdlgklmfdg" } }
            ));

            daprClient.Setup(x => x.InvokeMethodAsync<ApiKey>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Aborted, "")));

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: tenantId,
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues("global") },
                    { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.False(authenticateResult.Succeeded);
            Assert.NotNull(authenticateResult.Failure);
            Assert.Equal("Invalid API Key provided.", authenticateResult.Failure!.Message);
        }

        [Fact]
        public async Task Authenticate_With_ApiKey_Not_Found_RPC_Exception()
        {
            string tenantId = "global";
            string apiKey = $"{tenantId}.ABCDEFGH";

            var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
            options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
            var loggerFactory = new Mock<ILoggerFactory>();
            var encoder = new Mock<UrlEncoder>();
            var clock = new Mock<ISystemClock>();
            var apiKeyCacheService = new Mock<CacheService<ApiKey>>();
            var roleService = new Mock<IRoleService>();
            var daprClient = new Mock<DaprClient>();
            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
                new Role(RoleConstant.USER, UpperRoleCode: RoleConstant.TENANT_MANAGER),
                new Role(RoleConstant.TENANT_MANAGER, UpperRoleCode: RoleConstant.ADMIN),
                new Role(RoleConstant.ADMIN)
            });

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string> { { ConfigConstant.MicroserviceApiKey, "fgdfgkfdgmfmdkgmkdlgklmfdg" } }
            ));

            daprClient.Setup(x => x.InvokeMethodAsync<ApiKey>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.NotFound, "")));

            var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
                options.Object,
                loggerFactory.Object,
                encoder.Object,
                clock.Object,
                apiKeyCacheService.Object,
                roleService.Object,
                daprClient.Object,
                logger.Object
            );

            var context = Fixture.CreateHttpContext(
                tenantId: tenantId,
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER },
                headers: new()
                {
                    { HttpHeaderConstant.TenantId, new StringValues("global") },
                    { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
                }
            );

            await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

            var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

            Assert.NotNull(authenticateResult);
            Assert.False(authenticateResult.Succeeded);
            Assert.NotNull(authenticateResult.Failure);
            Assert.Equal("Invalid API Key provided.", authenticateResult.Failure!.Message);
        }
    }
}
