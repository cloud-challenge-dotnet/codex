using Codex.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace Codex.Core.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TenantAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public TenantAuthorizeAttribute() : base()
        {
        }

        public TenantAuthorizeAttribute(string policy) : base(policy)
        {
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return;
            }

            if (context.HttpContext.Request.Headers.TryGetValue(HttpHeaderConstant.TenantId, out var tenantIdValues))
            {
                var tenantId = tenantIdValues.FirstOrDefault();
                var claimTenantId = user.FindAll(ClaimConstant.TenantId)?.FirstOrDefault()?.Value;

                if (!string.IsNullOrWhiteSpace(tenantId) && tenantId != claimTenantId)
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}
