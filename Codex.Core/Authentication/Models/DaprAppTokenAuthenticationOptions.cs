using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;

namespace Codex.Core.Authentication.Models;

[ExcludeFromCodeCoverage]
public class DaprAppTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "dapr-api-token";
    public static string Scheme => DefaultScheme;
    public static string AuthenticationType => DefaultScheme;
}