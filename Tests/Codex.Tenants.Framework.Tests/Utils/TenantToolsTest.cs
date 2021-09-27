using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Models.Exceptions;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Exceptions;
using Codex.Tenants.Framework.Resources;
using Codex.Tenants.Framework.Utils;
using Codex.Tests.Framework;
using Dapr.Client;
using Grpc.Core;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Tenants.Framework.Tests.Utils
{
    public class TenantToolsTest : IClassFixture<Fixture>
    {
        private readonly ILogger<TenantToolsTest> _logger;

        private readonly IStringLocalizer<TenantFrameworkResource> _sl;

        public TenantToolsTest(ILogger<TenantToolsTest> logger)
        {
            _logger = logger;

            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            _sl = new StringLocalizer<TenantFrameworkResource>(factory);
        }

        [Fact]
        public async Task SearchTenantById()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<TenantCacheService>();

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() { { ConfigConstant.MicroserviceApiKey, "" } }
            ));

            daprClient.Setup(x => x.CreateInvokeMethodRequest(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new HttpRequestMessage());

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Tenant("global", "", null)));

            var tenant = await TenantTools.SearchTenantByIdAsync(_logger, _sl, tenantCacheService.Object, daprClient.Object, tenantId);

            Assert.NotNull(tenant);
            Assert.Equal("global", tenant.Id);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
            daprClient.Verify(v => v.InvokeMethodAsync<Tenant>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
            tenantCacheService.Verify(v => v.UpdateCacheAsync(daprClient.Object, It.IsAny<string>(), It.IsAny<Tenant>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Inside_Cache()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<TenantCacheService>();

            tenantCacheService.Setup(x => x.GetCacheAsync(
                daprClient.Object, It.IsAny<string>()))
                .Returns(Task.FromResult<Tenant?>(new Tenant("global", "", null)));

            var tenant = await TenantTools.SearchTenantByIdAsync(_logger, _sl, tenantCacheService.Object, daprClient.Object, tenantId);

            Assert.NotNull(tenant);
            Assert.Equal("global", tenant.Id);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
            daprClient.Verify(v => v.InvokeMethodAsync<Tenant>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
            tenantCacheService.Verify(v => v.UpdateCacheAsync(daprClient.Object, It.IsAny<string>(), It.IsAny<Tenant>()), Times.Never);
        }

        [Fact]
        public async Task SearchTenantById_Generic_Exception()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<TenantCacheService>();

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(new System.Exception("invalid tenant"));

            var technicalException = await Assert.ThrowsAsync<TechnicalException>(
                async () => await TenantTools.SearchTenantByIdAsync(_logger, _sl, tenantCacheService.Object, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Aborted_RpcException()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            var tenantCacheService = new Mock<TenantCacheService>();

            daprClient.Setup(x => x.CreateInvokeMethodRequest(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new HttpRequestMessage());

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Aborted, "")));

            var technicalException = await Assert.ThrowsAsync<TechnicalException>(
                async () => await TenantTools.SearchTenantByIdAsync(_logger, _sl, tenantCacheService.Object, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SearchTenantById_Notfound_RpcException()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>();

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() { { ConfigConstant.MicroserviceApiKey, "" } }
            ));

            var tenantCacheService = new Mock<TenantCacheService>();

            daprClient.Setup(x => x.CreateInvokeMethodRequest(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new HttpRequestMessage());

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.NotFound, "")));

            var invalidTenantIdException = await Assert.ThrowsAsync<InvalidTenantIdException>(
                async () => await TenantTools.SearchTenantByIdAsync(_logger, _sl, tenantCacheService.Object, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", invalidTenantIdException.Code);

            tenantCacheService.Verify(v => v.GetCacheAsync(daprClient.Object, It.IsAny<string>()), Times.Once);
            daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
