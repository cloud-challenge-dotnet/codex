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
    public class MicroServiceTenantToolsTest : IClassFixture<Fixture>
    {
        private readonly ILogger<MicroServiceTenantToolsTest> _logger;

        public MicroServiceTenantToolsTest(ILogger<MicroServiceTenantToolsTest> logger)
        {
            _logger = logger;
        }

        [Fact]
        public async Task SearchTenantById()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>() { DefaultValue = DefaultValue.Mock };
            await daprClient.Object.InvokeMethodAsync<Tenant>("tenantapi", $"Tenant/{tenantId}", new HTTPExtension() { Verb = HTTPVerb.Get });

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Tenant>(new Tenant("global", "", null)));

            var tenant = await MicroServiceTenantTools.SearchTenantByIdAsync(_logger, daprClient.Object, tenantId);

            Assert.NotNull(tenant);
            Assert.Equal("global", tenant.Id);
        }

        [Fact]
        public async Task SearchTenantById_Generic_Exception()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>() { DefaultValue = DefaultValue.Mock };
            await daprClient.Object.InvokeMethodAsync<Tenant>("tenantapi", $"Tenant/{tenantId}", new HTTPExtension() { Verb = HTTPVerb.Get });

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new System.Exception("invalid tenant"));

            var technicalException = await Assert.ThrowsAsync<TechnicalException>(
                async () => await MicroServiceTenantTools.SearchTenantByIdAsync(_logger, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);
        }

        [Fact]
        public async Task SearchTenantById_Aborted_RpcException()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>() { DefaultValue = DefaultValue.Mock };
            await daprClient.Object.InvokeMethodAsync<Tenant>("tenantapi", $"Tenant/{tenantId}", new HTTPExtension() { Verb = HTTPVerb.Get });

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Aborted, "")));

            var technicalException = await Assert.ThrowsAsync<TechnicalException>(
                async () => await MicroServiceTenantTools.SearchTenantByIdAsync(_logger, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", technicalException.Code);
        }

        [Fact]
        public async Task SearchTenantById_Notfound_RpcException()
        {
            string tenantId = "global";

            var daprClient = new Mock<DaprClient>() { DefaultValue = DefaultValue.Mock };
            await daprClient.Object.InvokeMethodAsync<Tenant>("tenantapi", $"Tenant/{tenantId}", new HTTPExtension() { Verb = HTTPVerb.Get });

            daprClient.Setup(x => x.InvokeMethodAsync<Tenant>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HTTPExtension>(), It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.NotFound, "")));

            var invalidTenantIdException = await Assert.ThrowsAsync<InvalidTenantIdException>(
                async () => await MicroServiceTenantTools.SearchTenantByIdAsync(_logger, daprClient.Object, tenantId)
            );

            Assert.Equal("TENANT_NOT_FOUND", invalidTenantIdException.Code);
        }
    }
}
