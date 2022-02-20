using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Models.Users;
using Codex.Users.Api.Exceptions;
using Codex.Users.Api.Resources;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Codex.Core.Cache;

namespace Codex.Users.Api.Services.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;

    private readonly IConfiguration _configuration;

    private readonly DaprClient _daprClient;

    private readonly IPasswordHasher _passwordHasher;

    private readonly IUserService _userService;

    private readonly IRoleService _roleService;

    private readonly IStringLocalizer<UserResource> _sl;

    private readonly ITenantCacheService _tenantCacheService;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        DaprClient daprClient,
        IPasswordHasher passwordHasher,
        IUserService userService,
        IConfiguration configuration,
        IRoleService roleService,
        IStringLocalizer<UserResource> sl,
        ITenantCacheService tenantCacheService)
    {
        _logger = logger;
        _daprClient = daprClient;
        _passwordHasher = passwordHasher;
        _userService = userService;
        _configuration = configuration;
        _roleService = roleService;
        _sl = sl;
        _tenantCacheService = tenantCacheService;
    }

    public async Task<Auth> AuthenticateAsync(UserLogin userLogin)
    {
        if (string.IsNullOrWhiteSpace(userLogin.Login) || string.IsNullOrWhiteSpace(userLogin.Password))
            throw new InvalidCredentialsException(_sl[UserResource.InvalidLogin], code: "INVALID_LOGIN");

        Tenant tenant = await _tenantCacheService.GetTenantAsync(userLogin.TenantId);

        var userCriteria = new UserCriteria(Login: userLogin.Login);
        var user = (await _userService.FindAllAsync(userCriteria)).FirstOrDefault(u => u.Login == userLogin.Login);

        if (user == null && userLogin.TenantId != "global")
        {
            // Search user on global instance (user inter tenant for administration)
            try
            {
                string methodNameWithParams = QueryHelpers.AddQueryString("User",
                    new Dictionary<string, string?>()
                    {
                        {"Login", userLogin.Login }
                    }
                );

                var requestMessage = _daprClient.CreateInvokeMethodRequest(HttpMethod.Get, ApiNameConstant.UserApi, methodNameWithParams);
                requestMessage.Headers.Add(HttpHeaderConstant.TenantId, "global");
                List<User> userList = await _daprClient.InvokeMethodAsync<List<User>>(requestMessage);
                user = userList.FirstOrDefault(u => u.Login == userLogin.Login);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to find User '{UserLogin}' inside global instance", userLogin.Login);
            }
        }

        if (user == null)
            throw new InvalidCredentialsException(_sl[UserResource.InvalidLogin], code: "INVALID_LOGIN");

        if (!user.Active)
            throw new DisabledUserException(_sl[UserResource.UserIsDisabled], code: "DISABLED_USER");

        if (!await CheckPasswordAsync(user.PasswordHash, userLogin.Password))
            throw new InvalidCredentialsException(_sl[UserResource.InvalidLogin], code: "INVALID_LOGIN");

        Auth auth = new(Id: user.Id!, Login: user.Login, Token: CreateToken(user, tenant));

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
    private string CreateToken(User user, Tenant tenant)
    {
        user = CompleteUserWithParentRoles(user);
        List<Claim> claimList = new()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimConstant.TenantId, tenant.Id)
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

        user.Roles.ForEach(roleCode =>
        {
            var role = roles.FirstOrDefault(r => r.Code == roleCode);
            if (role != null)
            {
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
            roleList.Add(parentRole);
            roleList.AddRange(GetLowerRoles(roles, parentRole));
        }
        return roleList;
    }
}