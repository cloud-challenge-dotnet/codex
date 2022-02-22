using Codex.Models.Tenants;
using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Framework.Models;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Codex.Tenants.Framework.Tests;

public class TenantAccessorTest : IClassFixture<Fixture>
{
    [Fact]
    public void Get_Null_Tenant()
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns<HttpContext>(
            null
        );

        TenantAccessor tenantAccessor = new(httpContextAccessor.Object);

        Tenant? tenant = tenantAccessor.Tenant;

        Assert.Null(tenant);
    }

    [Fact]
    public void Get_Null_Tenant2()
    {
        var httpContext = new DefaultHttpContext();

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(
            httpContext
        );

        TenantAccessor tenantAccessor = new(httpContextAccessor.Object);

        Tenant? tenant = tenantAccessor.Tenant;

        Assert.Null(tenant);
    }

    [Fact]
    public void GetTenant()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[Constants.HttpContextTenantKey] = new Tenant("tenant", "my tenant");

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(
            httpContext
        );

        TenantAccessor tenantAccessor = new(httpContextAccessor.Object);

        Tenant? tenant = tenantAccessor.Tenant;

        Assert.NotNull(tenant);
        Assert.Equal("tenant", tenant!.Id);
        Assert.Equal("my tenant", tenant.Name);
    }
}