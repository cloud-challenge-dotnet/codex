using Microsoft.AspNetCore.Http;
using Codex.Tenants.Models;
using System.Threading.Tasks;
using Codex.Tenants.Framework.Models;
using Codex.Tenants.Framework.Interfaces;

namespace Codex.Tenants.Framework
{
    internal class TenantMiddleware<T> where T : Tenant
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
                var tenantService = context.RequestServices.GetService(typeof(ITenantAccessService)) as ITenantAccessService;
                if (tenantService != null)
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
