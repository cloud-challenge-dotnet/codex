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
using System.Threading;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;

namespace Codex.Tests.Framework;

[ExcludeFromCodeCoverage]
public class Fixture
{
    public Fixture(IServiceProvider services)
    {
        _services = services;

        var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", camelCaseConventionPack, _ => true);
    }

    private readonly IServiceProvider _services;

    public IServiceProvider Services => _services;

    public static ServerCallContext CreateServerCallContext(HttpContext? httpContext = null)
    {
        var serviceCallContext = TestServerCallContext.Create(
            "fooMethod", 
            null, 
            DateTime.UtcNow.AddHours(1), 
            new Metadata(),
            CancellationToken.None, 
            "127.0.0.1", 
            null, 
            null, 
            (_) => TaskUtils.CompletedTask, () => new WriteOptions(), (_) => { }
        );

        if (httpContext != null)
        {
            serviceCallContext.UserState["__HttpContext"] = httpContext;
        }

        return serviceCallContext;
    }

    public static HttpContext CreateHttpContext(string tenantId, string? userId = null, string? userName = null,
        List<string>? roles = null, Dictionary<string, StringValues>? headers = null)
    {
        List<Claim> claimList = new()
        {
            new Claim(ClaimTypes.NameIdentifier, userId??""),
            new Claim(ClaimTypes.Name, userName??""),
            new Claim(ClaimConstant.TenantId, tenantId),
        };

        if (roles != null)
        {
            claimList.AddRange(roles.Select(r =>
                new Claim(ClaimTypes.Role, r)
            ));
        }

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