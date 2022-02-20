using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Models;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework;

[ExcludeFromCodeCoverage]
internal class TenantMiddleware
{
    private readonly RequestDelegate? _next;

    public TenantMiddleware(RequestDelegate? next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Items.ContainsKey(Constants.HttpContextTenantKey))
        {
            if (context.RequestServices.GetService(typeof(ITenantAccessService)) is ITenantAccessService tenantService)
            {
                var tenant = await tenantService.GetTenantAsync();
                context.Items.Add(Constants.HttpContextTenantKey, tenant);
            }
        }

        //Continue processing
        if (_next != null)
            await _next(context);
    }
}