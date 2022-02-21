using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Codex.Core.Authentication;
using Codex.Core.Authentication.Models;
using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Models.Security;
using Codex.Tests.Framework;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Codex.Core.Tests.ApiKeys;

public class ApiKeyAuthenticationHandlerIt : IClassFixture<Fixture>
{
    [Fact]
    public async Task Authenticate_Without_Api_Key()
    {
        var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
        var loggerFactory = new Mock<ILoggerFactory>();
        var encoder = new Mock<UrlEncoder>();
        var clock = new Mock<ISystemClock>();
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();
        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = new DefaultHttpContext();

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

        apiKeyCacheService.Verify(x => x.GetApiKeyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

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
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();
        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = Fixture.CreateHttpContext(
            tenantId: "global",
            userId: "Id1",
            userName: "login",
            roles: new() { RoleConstant.TenantManager },
            headers: new()
            {
                { HttpHeaderConstant.TenantId, new StringValues("global") },
                { HttpHeaderConstant.ApiKey, new StringValues("       ") }
            }
        );

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

        apiKeyCacheService.Verify(x => x.GetApiKeyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        Assert.NotNull(authenticateResult);
        Assert.False(authenticateResult.Succeeded);
        Assert.True(authenticateResult.None);
    }


    [Fact]
    public async Task Authenticate_With_Api_Key_Without_Tenant_Id()
    {
        string tenantId = "global";
        string apiKey = "fgdfgkfdgmfmdkgmkdlgklmfdg";
        var options = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        options.Setup(x => x.Get(It.IsAny<string>())).Returns(new ApiKeyAuthenticationOptions());
        var loggerFactory = new Mock<ILoggerFactory>();
        var encoder = new Mock<UrlEncoder>();
        var clock = new Mock<ISystemClock>();
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();
        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = Fixture.CreateHttpContext(
            tenantId: tenantId,
            userId: "Id1",
            userName: "login",
            roles: new() { RoleConstant.TenantManager },
            headers: new()
            {
                { HttpHeaderConstant.TenantId, new StringValues(tenantId) },
                { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
            }
        );

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

        apiKeyCacheService.Verify(x => x.GetApiKeyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        Assert.NotNull(authenticateResult);
        Assert.False(authenticateResult.Succeeded);
        Assert.NotNull(authenticateResult.Failure);
        Assert.Equal("Invalid API Key provided.", authenticateResult.Failure!.Message);
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
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();

        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
            new Role(RoleConstant.User, UpperRoleCode: RoleConstant.TenantManager),
            new Role(RoleConstant.TenantManager, UpperRoleCode: RoleConstant.Admin),
            new Role(RoleConstant.Admin)
        });

        apiKeyCacheService.Setup(x => x.GetApiKeyAsync("ABCDEFGH", tenantId)).Returns(
            Task.FromResult(new ApiKey(apiKey, "test api key", new(){RoleConstant.User}))
        );

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = Fixture.CreateHttpContext(
            tenantId: tenantId,
            userId: "Id1",
            userName: "login",
            roles: new() { RoleConstant.TenantManager },
            headers: new()
            {
                { HttpHeaderConstant.TenantId, new StringValues("global") },
                { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
            }
        );

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();
        
        apiKeyCacheService.Verify(x => x.GetApiKeyAsync("ABCDEFGH", tenantId), "GetApiKeyAsync was never invoked");

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
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();
        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
            new Role(RoleConstant.User, UpperRoleCode: RoleConstant.TenantManager),
            new Role(RoleConstant.TenantManager, UpperRoleCode: RoleConstant.Admin),
            new Role(RoleConstant.Admin)
        });
        
        apiKeyCacheService.Setup(x => x.GetApiKeyAsync("ABCDECCDFSFDF", tenantId))
            .Throws(new RpcException(new Status(StatusCode.Unknown, "Api Key is not found")));

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = Fixture.CreateHttpContext(
            tenantId: tenantId,
            userId: "Id1",
            userName: "login",
            roles: new() { RoleConstant.TenantManager },
            headers: new()
            {
                { HttpHeaderConstant.TenantId, new StringValues("global") },
                { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
            }
        );

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();
        
        apiKeyCacheService.Verify(x => x.GetApiKeyAsync("ABCDECCDFSFDF", tenantId), "GetApiKeyAsync was never invoked");
        
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
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();
        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
            new Role(RoleConstant.User, UpperRoleCode: RoleConstant.TenantManager),
            new Role(RoleConstant.TenantManager, UpperRoleCode: RoleConstant.Admin),
            new Role(RoleConstant.Admin)
        });
        
        apiKeyCacheService.Setup(x => x.GetApiKeyAsync("ABCDEFGH", tenantId))
            .Throws(new RpcException(new Status(StatusCode.Internal, "ERROR")));

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = Fixture.CreateHttpContext(
            tenantId: tenantId,
            userId: "Id1",
            userName: "login",
            roles: new() { RoleConstant.TenantManager },
            headers: new()
            {
                { HttpHeaderConstant.TenantId, new StringValues("global") },
                { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
            }
        );

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

        
        apiKeyCacheService.Verify(x => x.GetApiKeyAsync("ABCDEFGH", tenantId), "GetApiKeyAsync was never invoked");

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
        var apiKeyCacheService = new Mock<IApiKeyCacheService>();
        var roleService = new Mock<IRoleService>();
        var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        roleService.Setup(x => x.GetRoles()).Returns(new List<Role>() {
            new Role(RoleConstant.User, UpperRoleCode: RoleConstant.TenantManager),
            new Role(RoleConstant.TenantManager, UpperRoleCode: RoleConstant.Admin),
            new Role(RoleConstant.Admin)
        });

        apiKeyCacheService.Setup(x => x.GetApiKeyAsync("ABCDEFGH", tenantId))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Api Key is not found")));

        var apiKeyAuthenticationHandler = new ApiKeyAuthenticationHandler(
            options.Object,
            loggerFactory.Object,
            encoder.Object,
            clock.Object,
            apiKeyCacheService.Object,
            roleService.Object
        );

        var context = Fixture.CreateHttpContext(
            tenantId: tenantId,
            userId: "Id1",
            userName: "login",
            roles: new() { RoleConstant.TenantManager },
            headers: new()
            {
                { HttpHeaderConstant.TenantId, new StringValues("global") },
                { HttpHeaderConstant.ApiKey, new StringValues(apiKey) }
            }
        );

        await apiKeyAuthenticationHandler.InitializeAsync(new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, null, typeof(ApiKeyAuthenticationHandler)), context);

        var authenticateResult = await apiKeyAuthenticationHandler.AuthenticateAsync();

        apiKeyCacheService.Verify(x => x.GetApiKeyAsync("ABCDEFGH", tenantId), "GetApiKeyAsync was never invoked");

        Assert.NotNull(authenticateResult);
        Assert.False(authenticateResult.Succeeded);
        Assert.NotNull(authenticateResult.Failure);
        Assert.Equal("Invalid API Key provided.", authenticateResult.Failure!.Message);
    }
}