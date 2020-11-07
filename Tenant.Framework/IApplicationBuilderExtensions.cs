﻿using Microsoft.AspNetCore.Builder;

namespace Codex.Tenants.Framework
{
    /// <summary>
    /// Nice method to register our middleware
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Use the Teanant Middleware to process the request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder)
            => builder.UseMiddleware<TenantMiddleware>();

        public static IApplicationBuilder UseMultiTenantContainer(this IApplicationBuilder builder)
            => builder.UseMiddleware<MultitenantContainerMiddleware>();
    }
}
