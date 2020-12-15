using Codex.Core.Models;
using Microsoft.AspNetCore.Http;
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

        public static HttpContext CreateHttpContext(string tenantId, string userId, string userName, List<string> roles)
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

            return new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(claimList, "TestAuthType")
                )
            };
        }
    }
}