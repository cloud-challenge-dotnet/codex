
using Microsoft.AspNetCore.Http;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Models;
using System.Security.Claims;
using System.Linq;

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
        public static Tenant? GetTenant(this HttpContext context)
        {
            if (!context.Items.ContainsKey(Constants.HttpContextTenantKey))
                return null;
            return context.Items[Constants.HttpContextTenantKey] as Tenant;
        }

        /// <summary>
        /// Returns the current user id
        /// </summary>
        public static string? GetUserId(this HttpContext context)
        {
            if (context?.User == null)
                return null;

            return context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Returns the current user name
        /// </summary>
        public static string? GetUserName(this HttpContext context)
        {
            if (context.User == null)
                return null;

            return context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }
    }
}