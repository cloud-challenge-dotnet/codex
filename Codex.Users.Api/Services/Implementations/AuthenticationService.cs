using Codex.Tenants.Framework.Utils;
using Codex.Models.Users;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Codex.Users.Api.Exceptions;
using System;
using Codex.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Collections.Generic;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Core.Models;
using Codex.Core.Cache;
using Codex.Core.Roles.Interfaces;
using Dapr.Client.Http;
using MongoDB.Bson;

namespace Codex.Users.Api.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;

        private readonly IConfiguration _configuration;

        private readonly DaprClient _daprClient;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IUserService _userService;

        private readonly IRoleService _roleService;

        private readonly TenantCacheService _tenantCacheService;

        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            DaprClient daprClient,
            IPasswordHasher passwordHasher,
            IUserService userService,
            IConfiguration configuration,
            IRoleService roleService,
            TenantCacheService tenantCacheService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _passwordHasher = passwordHasher;
            _userService = userService;
            _configuration = configuration;
            _roleService = roleService;
            _tenantCacheService = tenantCacheService;
        }

        public async Task<Auth> AuthenticateAsync(UserLogin userLogin)
        {
            if (string.IsNullOrWhiteSpace(userLogin.Login) || string.IsNullOrWhiteSpace(userLogin.Password))
                throw new InvalidCredentialsException("Invalid login", code: "INVALID_LOGIN");

            Tenant tenant = await TenantTools.SearchTenantByIdAsync(_logger, _tenantCacheService, _daprClient, userLogin.TenantId);

            var userCriteria = new UserCriteria(Login: userLogin.Login);
            var user = (await _userService.FindAllAsync(userCriteria)).Where(u => u.Login == userLogin.Login).FirstOrDefault();

            if (user == null && userLogin.TenantId != "global")
            {
                // Search user on global instance (user inter tenant for administration
                var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.MicroserviceApiKey);
                var microserviceApiKey = secretValues[ConfigConstant.MicroserviceApiKey];

                try {
                    user = (await _daprClient.InvokeMethodAsync<List<User>>(ApiNameConstant.UserApi, "User",
                        httpExtension: new HTTPExtension()
                        {
                            Verb = HTTPVerb.Get,
                            QueryString = new Dictionary<string, string>()
                            {
                                { "Login", userLogin.Login }
                            },
                            Headers = {
                                { HttpHeaderConstant.TenantId, "global" },
                                { HttpHeaderConstant.ApiKey, $"global.{microserviceApiKey}" }
                            }
                        }
                    )).Where(u => u.Login == userLogin.Login).FirstOrDefault();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Unable to find User '{userLogin.Login}' inside global instance");
                }
            }

            if(user == null)
                throw new InvalidCredentialsException("Invalid login", code: "INVALID_LOGIN");

            if (!user.Active)
                throw new DisabledUserException($"User {user.Id} is disabled", code: "DISABLED_USER");

            if (!await CheckPasswordAsync(user.PasswordHash, userLogin.Password))
                throw new InvalidCredentialsException("Invalid login", code: "INVALID_LOGIN");

            Auth auth = new(Id: ((ObjectId)user.Id!).ToString() ?? "", Login: user.Login, Token: CreateToken(user, tenant));

            return await Task.FromResult(auth);
        }

        private async Task<bool> CheckPasswordAsync(string? passwordHash, string password)
        {
            var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.PasswordSalt);

            var salt = secretValues[ConfigConstant.PasswordSalt];

            string generatePasswordHash = _passwordHasher.GenerateHash(password, salt);
            return passwordHash == generatePasswordHash;
        }

        private const double EXPIRE_HOURS = 1.0;
        public string CreateToken(User user, Tenant tenant)
        {
            user = CompleteUserWithParentRoles(user);
            List<Claim> claimList = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id?.ToString() ?? ""),
                new Claim(ClaimTypes.Name, user.Login!),
                new Claim(ClaimConstant.TenantId, tenant.Id!)
            };
            claimList.AddRange(user.Roles.Select(r =>
                new Claim(ClaimTypes.Role, r)
            ));

            var key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>(ConfigConstant.JwtSecretKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
               
                Subject = new ClaimsIdentity(claimList.ToArray()),
                Expires = DateTime.UtcNow.AddHours(EXPIRE_HOURS),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        private User CompleteUserWithParentRoles(User user)
        {
            var roles = _roleService.GetRoles();

            var completedRoles = new List<string>();

            user.Roles.ForEach(roleCode => {
                var role = roles.FirstOrDefault(r => r.Code == roleCode);
                if(role != null) {
                    completedRoles.Add(roleCode);
                    completedRoles.AddRange(GetLowerRoles(roles, role).Select(r => r.Code));
                }
            });

            return user with { Roles = completedRoles.Distinct().ToList() };
        }

        private List<Role> GetLowerRoles(List<Role> roles, Role role)
        {
            List<Role> roleList = new();
            var parentRoles = roles.Where(r => r.UpperRoleCode == role.Code);
            foreach (var parentRole in parentRoles)
            {
                if (parentRole != null)
                {
                    roleList.Add(parentRole);
                    roleList.AddRange(GetLowerRoles(roles, parentRole));
                }
            }
            return roleList;
        }
    }
}
