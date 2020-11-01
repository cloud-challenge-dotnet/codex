using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Codex.Tenants.Framework.Implementations;
using System;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework
{
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
            await next.Invoke(context);

            //Continue processing
            if (next != null)
                await next(context);
        }
    }
}
