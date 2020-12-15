using Microsoft.AspNetCore.Authentication;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Core.ApiKeys.Models
{
    [ExcludeFromCodeCoverage]
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";
        public static string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
    }
}
