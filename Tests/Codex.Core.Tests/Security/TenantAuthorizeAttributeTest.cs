using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Xunit;

namespace Codex.Core.Tests.Security
{
    public class TenantAuthorizeAttributeTest : IClassFixture<Fixture>
    {
        public TenantAuthorizeAttributeTest()
        {
        }

        [Fact]
        public void OnAuthorization()
        {
            ActionContext actionContext = new(
                httpContext: Fixture.CreateHttpContext(
                    tenantId: "global",
                    userId: "Id1",
                    userName: "login",
                    roles: new() { RoleConstant.TENANT_MANAGER },
                    headers: new() {
                        { "tenantId", new StringValues("global") }
                    }
                ),
                new(),
                new(),
                new()
            );

            AuthorizationFilterContext authorizationFilterContext = new(actionContext, new List<IFilterMetadata>());

            TenantAuthorizeAttribute tenantAuthorizeAttribute = new();

            tenantAuthorizeAttribute.OnAuthorization(authorizationFilterContext);

            Assert.Null(authorizationFilterContext.Result);
        }

        [Fact]
        public void OnAuthorization_UnauthorizedResult()
        {
            ActionContext actionContext = new(
                httpContext: Fixture.CreateHttpContext(
                    tenantId: "global",
                    userId: "Id1",
                    userName: "login",
                    roles: new() { RoleConstant.TENANT_MANAGER },
                    headers: new()
                    {
                        { "tenantId", new StringValues("demo") }
                    }
                ),
                new(),
                new(),
                new()
            );

            AuthorizationFilterContext authorizationFilterContext = new(actionContext, new List<IFilterMetadata>());

            TenantAuthorizeAttribute tenantAuthorizeAttribute = new();

            tenantAuthorizeAttribute.OnAuthorization(authorizationFilterContext);

            Assert.NotNull(authorizationFilterContext.Result);
            Assert.IsType<UnauthorizedResult>(authorizationFilterContext.Result);
        }

        [Fact]
        public void OnAuthorization_Identity_Null()
        {
            ActionContext actionContext = new(
                httpContext: new DefaultHttpContext
                {
                        User = new ClaimsPrincipal()
                },
                new(),
                new(),
                new()
            );

            AuthorizationFilterContext authorizationFilterContext = new(actionContext, new List<IFilterMetadata>());

            TenantAuthorizeAttribute tenantAuthorizeAttribute = new();

            tenantAuthorizeAttribute.OnAuthorization(authorizationFilterContext);

            Assert.Null(authorizationFilterContext.Result);
        }

        [Fact]
        public void OnAuthorization_Identity_Is_Not_Authenticated()
        {
            var identity = new Mock<IIdentity>();
            ActionContext actionContext = new(
                httpContext: new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        identity.Object
                    )
                },
                new(),
                new(),
                new()
            );

            AuthorizationFilterContext authorizationFilterContext = new(actionContext, new List<IFilterMetadata>());

            TenantAuthorizeAttribute tenantAuthorizeAttribute = new(policy: "");

            tenantAuthorizeAttribute.OnAuthorization(authorizationFilterContext);

            Assert.Null(authorizationFilterContext.Result);
        }
    }
}
