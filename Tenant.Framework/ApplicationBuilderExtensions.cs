using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Framework;

/// <summary>
/// Nice method to register our middleware
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Use the Tenant Middleware to process the request
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder)
        => builder.UseMiddleware<TenantMiddleware>();

    public static IApplicationBuilder UseMultiTenantContainer(this IApplicationBuilder builder)
        => builder.UseMiddleware<MultiTenantContainerMiddleware>();
}