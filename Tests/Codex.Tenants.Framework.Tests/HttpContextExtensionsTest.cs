
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Models;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace Codex.Tenants.Framework.Tests;

public class HttpContextExtensionsTest : IClassFixture<Fixture>
{
    [Fact]
    public void GetTenant()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items.Add(Constants.HttpContextTenantKey, new Tenant() { Id = "global" });

        Tenant? tenant = httpContext.GetTenant();

        Assert.NotNull(tenant);
        Assert.Equal("global", tenant!.Id);
    }

    [Fact]
    public void GetTenant_Null()
    {
        var httpContext = new DefaultHttpContext();

        Tenant? tenant = httpContext.GetTenant();

        Assert.Null(tenant);
    }


    [Fact]
    public void GetUserId()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>() { new(ClaimTypes.NameIdentifier, "Id1") }, "TestAuthType")
            )
        };

        string? userId = httpContext.GetUserId();

        Assert.NotNull(userId);
        Assert.Equal("Id1", userId);
    }

    [Fact]
    public void GetUserId_Null()
    {
        var httpContext = new DefaultHttpContext();

        string? userId = httpContext.GetUserId();

        Assert.Null(userId);
    }

    [Fact]
    public void GetUserId_Null_HttpContext()
    {
        HttpContext? httpContext = null;

        string? userId = httpContext!.GetUserId();

        Assert.Null(userId);
    }

    [Fact]
    public void GetUserName()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim>() { new(ClaimTypes.Name, "User name") }, "TestAuthType")
            )
        };

        string? userName = httpContext.GetUserName();

        Assert.NotNull(userName);
        Assert.Equal("User name", userName);
    }

    [Fact]
    public void GetUserName_Null()
    {
        var httpContext = new DefaultHttpContext();

        string? userName = httpContext.GetUserName();

        Assert.Null(userName);
    }

    [Fact]
    public void GetUserName_Null_HttpContext()
    {
        HttpContext? httpContext = null;

        string? userName = httpContext!.GetUserName();

        Assert.Null(userName);
    }
}