using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

namespace Codex.Core.Authentication;

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
    public static IApplicationBuilder UseDaprApiToken(this IApplicationBuilder builder)
        => builder.UseMiddleware<DaprApiTokenMiddleware>();
}