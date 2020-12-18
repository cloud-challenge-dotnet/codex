using Codex.Core.Models;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace Codex.Tests.Framework
{
    [ExcludeFromCodeCoverage]
    public class Fixture
    {
        public Fixture(IServiceProvider services)
        {
            _services = services;

            var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);
        }

        private readonly IServiceProvider _services;

        public IServiceProvider Services
        {
            get => _services;
        }

        public static HttpContext CreateHttpContext(string tenantId, string userId, string userName,
            List<string> roles, Dictionary<string, StringValues>? headers = null)
        {
            List<Claim> claimList = new()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimConstant.TenantId, tenantId),
            };
            claimList.AddRange(roles.Select(r =>
                new Claim(ClaimTypes.Role, r)
            ));

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claimList, "TestAuthType")
                )
            };
            httpContext.Items.Add(Constants.HttpContextTenantKey, new Tenant() { Id = tenantId, Name = "tenantId" });

            if (headers != null)
            {
                foreach (var keyVal in headers)
                {
                    httpContext.Request.Headers.Append(keyVal.Key, keyVal.Value);
                }
            }

            return httpContext;
        }
    }
}