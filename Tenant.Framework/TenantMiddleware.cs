using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Codex.Tenants.Framework.Models;
using Codex.Tenants.Framework.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Framework
{
    [ExcludeFromCodeCoverage]
    internal class TenantMiddleware
    {
        private readonly RequestDelegate next;

        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Items.ContainsKey(Constants.HttpContextTenantKey))
            {
                if (context.RequestServices.GetService(typeof(ITenantAccessService)) is ITenantAccessService tenantService)
                {
                    context.Items.Add(Constants.HttpContextTenantKey, await tenantService.GetTenantAsync());
                }
            }

            //Continue processing
            if (next != null)
                await next(context);
        }
    }
}
