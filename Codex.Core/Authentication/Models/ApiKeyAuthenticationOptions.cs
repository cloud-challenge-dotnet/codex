using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;

namespace Codex.Core.Authentication.Models;

[ExcludeFromCodeCoverage]
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "API Key";
    public static string Scheme => DefaultScheme;
    public string AuthenticationType = DefaultScheme;
}