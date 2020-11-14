using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Codex.Tenants.Framework.Implementations;
using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Framework
{
    [ExcludeFromCodeCoverage]
    internal class MultitenantContainerMiddleware
    {
        private readonly RequestDelegate next;

        public MultitenantContainerMiddleware(RequestDelegate next)
        {
            this.next = next;
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
            if (next != null)
                await next.Invoke(context);
        }
    }
}
