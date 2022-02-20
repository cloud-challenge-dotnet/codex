using System;
using System.Diagnostics.CodeAnalysis;
using Codex.Core.Authentication.Models;
using Microsoft.AspNetCore.Authentication;

namespace Codex.Core.Authentication.Extensions;

[ExcludeFromCodeCoverage]
public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions>? options = null)
    {
        return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options);
    }
}
