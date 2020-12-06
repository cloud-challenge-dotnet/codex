using Codex.Core.Cache;
using Codex.Core.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Codex.Tenants.Framework.Utils;
using Codex.Tests.Framework;
using Dapr.Client;
using Dapr.Client.Http;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Framework.Tests.Utils
{
    public class TenantToolsTest : IClassFixture<Fixture>
    {
        private readonly ILogger<TenantToolsTest> _logger;

        public TenantToolsTest(ILogger<TenantToolsTest> logger)
        {
            _logger = logger;
        }

        [Fact]
        public async Task SearchTenantById()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<CacheService<Tenant>>();

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Tenant>(new Tenant("global", "", null)));

            var tenant = await TenantTools.SearchTenantByIdAsync(_logger, tenantCacheService.Object, daprClient.Object, tenantId);

            Assert.NotNull(tenant);
            Assert.Equal("global", tenant.Id);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Inside_Cache()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<CacheService<Tenant>>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("global", "", null)));

            var tenant = await TenantTools.SearchTenantByIdAsync(_logger, tenantCacheService.Object, daprClient.Object, tenantId);

            Assert.NotNull(tenant);
            Assert.Equal("global", tenant.Id);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Generic_Exception()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<CacheService<Tenant>>();

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new System.Exception("invalid tenant"));

            var technicalException = await Assert.ThrowsAsync<TechnicalException>(
                async () => await TenantTools.SearchTenantByIdAsync(_logger, tenantCacheService.Object, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Aborted_RpcException()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<CacheService<Tenant>>();

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Aborted, "")));

            var technicalException = await Assert.ThrowsAsync<TechnicalException>(
                async () => await TenantTools.SearchTenantByIdAsync(_logger, tenantCacheService.Object, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Notfound_RpcException()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<CacheService<Tenant>>();

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.NotFound, "")));

            var invalidTenantIdException = await Assert.ThrowsAsync<InvalidTenantIdException>(
                async () => await TenantTools.SearchTenantByIdAsync(_logger, tenantCacheService.Object, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", invalidTenantIdException.Code);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }
    }
}
