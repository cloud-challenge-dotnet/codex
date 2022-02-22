using Autofac.Extensions.DependencyInjection;
using Codex.Tenants.Framework.Implementations;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework;

[ExcludeFromCodeCoverage]
internal class MultiTenantContainerMiddleware
{
    private readonly RequestDelegate? _next;

    public MultiTenantContainerMiddleware(RequestDelegate? next)
    {
        this._next = next;
    }

    public async Task Invoke(HttpContext context,
        Func<MultiTenantContainer> multiTenantContainerAccessor)
    {
        //Set to current tenant container.
        //Begin new scope for request as ASP.NET Core standard scope is per-request
        context.RequestServices =
            new AutofacServiceProvider(multiTenantContainerAccessor()
                .GetCurrentTenantScope().BeginLifetimeScope());

        //Continue processing
        if (_next != null)
            await _next.Invoke(context);
    }
}