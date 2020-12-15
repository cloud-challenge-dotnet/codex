using Codex.Core.ApiKeys.Models;
using Codex.Core.Cache;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Models.Security;
using Dapr.Client;
using Dapr.Client.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Codex.Core.ApiKeys
{
    [ExcludeFromCodeCoverage]
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private const string ProblemDetailsContentType = "application/problem+json";
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly DaprClient _daprClient;
        private readonly CacheService<ApiKey> _apiKeyCacheService;
        private readonly IRoleService _roleService;
        private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            CacheService<ApiKey> apiKeyCacheService,
            IRoleService roleService,
            DaprClient daprClient,
            ILogger<ApiKeyAuthenticationHandler> logger) : base(options, loggerFactory, encoder, clock)
        {
            _apiKeyCacheService = apiKeyCacheService;
            _roleService = roleService;
            _daprClient = daprClient;
            _logger = logger;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.NoResult();
            }

            var splitValues = providedApiKey.Split('.');
            if(splitValues == null || splitValues.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid API Key provided.");
            }

            ApiKey? apiKey = null;
            string tenantId = splitValues[0];
            providedApiKey = splitValues[1];

            var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.MicroserviceApiKey);
            var microserviceApiKey = secretValues[ConfigConstant.MicroserviceApiKey];

            if (providedApiKey == microserviceApiKey)
            {
                apiKey = new(id: "inter-call-api-key", name: "Inter call Micro Services", new() { RoleConstant.ADMIN });
            }
            else
            {
                try
                {
                    string cacheKey = $"{CacheConstant.ApiKey_}{providedApiKey}";
                    apiKey = await _apiKeyCacheService.GetCacheAsync(_daprClient, cacheKey);
                    if (apiKey == null)
                    {
                        apiKey = await _daprClient.InvokeMethodAsync<ApiKey>("securityapi", $"ApiKey/{providedApiKey}",
                            new HTTPExtension()
                            {
                                Verb = HTTPVerb.Get,
                                Headers = {
                                    { "tenantId", tenantId },
                                    { ApiKeyHeaderName, $"{tenantId}.{microserviceApiKey}" }
                                }
                            }
                        );
                        await _apiKeyCacheService.UpdateCacheAsync(_daprClient, cacheKey, apiKey);
                    }
                }
                catch (Exception exception)
                {
                    if (exception is Grpc.Core.RpcException rpcException &&
                        rpcException.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
                    {
                        _logger.LogInformation(rpcException, $"ApiKey not found : '{apiKey?.Id}'");
                    }
                    else
                    {
                        _logger.LogError(exception, $"Unable to find ApiKey {apiKey?.Id}");
                    }
                    return AuthenticateResult.Fail("Invalid API Key provided.");
                }
            }

            if (apiKey != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, apiKey.Name),
                    new Claim(ClaimTypes.NameIdentifier, "API_KEY"),
                    new Claim(ClaimConstant.TenantId, tenantId!)
                };

                var roles = _roleService.GetRoles();

                var completedRoles = new List<string>();

                apiKey.Roles.ForEach(roleCode => {
                    var role = roles.FirstOrDefault(r => r.Code == roleCode);
                    if (role != null)
                    {
                        completedRoles.Add(roleCode);
                        completedRoles.AddRange(GetLowerRoles(roles, role).Select(r => r.Code));
                    }
                });

                apiKey = apiKey with { Roles = completedRoles.Distinct().ToList() };

                claims.AddRange(apiKey.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                var identities = new List<ClaimsIdentity> { identity };
                var principal = new ClaimsPrincipal(identities);
                var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.Scheme);

                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.Fail("Invalid API Key provided.");
        }

        private List<Role> GetLowerRoles(List<Role> roles, Role role)
        {
            List<Role> roleList = new();
            var parentRole = roles.FirstOrDefault(r => r.UpperRoleCode == role.Code);
            if (parentRole != null)
            {
                roleList.Add(parentRole);
                roleList.AddRange(GetLowerRoles(roles, parentRole));
            }
            return roleList;
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = ProblemDetailsContentType;
            var problemDetails = new CustomProblemDetails()
            {
                Status = 401,
                Title = "Unauthorized Api Key"
            };

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonSerializerOptions));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = ProblemDetailsContentType;
            var problemDetails = new CustomProblemDetails()
            {
                Status = 403,
                Title = "Forbidden Api Key"
            };

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IgnoreNullValues = true
            };
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonSerializerOptions));
        }
    }
}
