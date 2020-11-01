
using Microsoft.AspNetCore.Http;
using Codex.Tenants.Models;
using Codex.Tenants.Framework.Models;

namespace Codex.Tenants.Framework
{
    /// <summary>
    /// Extensions to HttpContext to make multi-tenancy easier to use
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Returns the current tenant
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Tenant? GetTenant(this HttpContext context)
        {
            if (!context.Items.ContainsKey(Constants.HttpContextTenantKey))
                return null;
            return context.Items[Constants.HttpContextTenantKey] as Tenant;
        }
    }
}