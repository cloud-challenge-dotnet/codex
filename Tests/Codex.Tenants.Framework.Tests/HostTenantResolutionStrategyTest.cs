using Codex.Tenants.Framework.Implementations;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Framework.Tests
{
    public class HostTenantResolutionStrategyTest : IClassFixture<Fixture>
    {
        public HostTenantResolutionStrategyTest()
        {
        }

        [Fact]
        public async Task GetTenantIdentifier()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["tenantId"] = "TenantTest";

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(
                httpContext
            );

            HostTenantResolutionStrategy tenantStrategy = new(httpContextAccessor.Object);

            string? tenantId = await tenantStrategy.GetTenantIdentifierAsync();

            Assert.NotNull(tenantId);
            Assert.Equal("TenantTest", tenantId);
        }

        [Fact]
        public async Task GetTenantIdentifier_Null_HttpRequest()
        {
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x.Request).Returns<HttpRequest>(
                null
            );

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(x => x.HttpContext).Returns(
                httpContext.Object
            );

            HostTenantResolutionStrategy tenantStrategy = new(httpContextAccessor.Object);

            string? tenantId = await tenantStrategy.GetTenantIdentifierAsync();

            Assert.Null(tenantId);
        }

        [Fact]
        public async Task GetTenantIdentifier_Null_HttpContext()
        {
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(x => x.Request).Returns<HttpRequest>(
                null
            );

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(x => x.HttpContext).Returns<HttpContext>(
                null
            );

            HostTenantResolutionStrategy tenantStrategy = new(httpContextAccessor.Object);

            string? tenantId = await tenantStrategy.GetTenantIdentifierAsync();

            Assert.Null(tenantId);
        }
    }
}
