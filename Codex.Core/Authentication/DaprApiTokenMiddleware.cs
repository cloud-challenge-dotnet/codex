using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Codex.Core.Authentication;

[ExcludeFromCodeCoverage]
internal class DaprApiTokenMiddleware
{
    private readonly RequestDelegate? _next;

    private const string DaprApiTokenKey = "dapr-api-token";
    private const string CodexDaprKey = "codex-dapr";

    public DaprApiTokenMiddleware(RequestDelegate? next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(DaprApiTokenKey, out var daprAppTokenHeaderValues))
        {
            if (daprAppTokenHeaderValues.Count > 0)
            {
                string? daprAppToken = daprAppTokenHeaderValues[0];
                //Verify origin
                var jwtHandler = new JwtSecurityTokenHandler();
                if (jwtHandler.CanReadToken(daprAppToken))
                {
                    var jwtToken = jwtHandler.ReadJwtToken(daprAppToken);
                    if (jwtToken.Issuer == CodexDaprKey) // security because user can add dapr-api-token in http request
                    {
                        // use dapr api token like Authorization Bearer token
                        context.Request.Headers.Add(HeaderNames.Authorization, $"Bearer {daprAppToken}");
                    }
                }
            }
        }

        //Continue processing
        if (_next != null)
            await _next(context);
    }
}