using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Models.Users;
using Codex.Tenants.Framework;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        [TenantAuthorize(Roles = "TENANT_MANAGER,USER")]
        public async Task<ActionResult<User>> FindOne(string id)
        {
            string? contextUserId = HttpContext.GetUserId();
            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER) && contextUserId != id)
            {
                return Unauthorized();
            }

            var user = await _userService.FindOneAsync(id);

            return user == null ? NotFound(id) : Ok(OffendUserFields(user));
        }

        [HttpGet]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<IEnumerable<User>>> FindAll([FromQuery] UserCriteria userCriteria)
        {
            var users = await _userService.FindAllAsync(userCriteria);

            return Ok(users.Select(user => OffendUserFields(user)).ToList());
        }

        [HttpPost]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserCreator userCreator)
        {
            string? tenantId = HttpContext.GetTenant()?.Id;
            var user = await _userService.CreateAsync(tenantId!, userCreator);
            user = OffendUserFields(user);

            return CreatedAtAction(nameof(FindOne), new { id = user.Id }, user);
        }

        [HttpPut("{userId}")]
        [TenantAuthorize(Roles = "TENANT_MANAGER,USER")]
        public async Task<ActionResult<User>> UpdateUser(string userId, [FromBody] User user)
        {
            string? contextUserId = HttpContext.GetUserId();
            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER) && contextUserId != userId)
            {
                return Unauthorized();
            }

            user = user with { Id = userId };

            User? userResult;
            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER) && contextUserId == userId)
            {
                userResult = await _userService.FindOneAsync(userId);
                if (userResult == null)
                {
                    return NotFound(userId);
                }
                user = user with
                { // current user without TENANT_MANAGER Role can't update all user fields
                    PasswordHash = null,
                    ActivationCode = null,
                    ActivationValidity = null,
                    Roles = userResult.Roles,
                    Active = userResult.Active
                };
            }

            userResult = await _userService.UpdateAsync(user);
            if (userResult == null)
            {
                return NotFound(userId);
            }

            return AcceptedAtAction(nameof(FindOne), new { id = user.Id }, OffendUserFields(userResult));
        }

        [HttpPut("{userId}/changePassword")]
        [TenantAuthorize(Roles = "TENANT_MANAGER,USER")]
        public async Task<ActionResult<User>> UpdatePassword(string userId, [FromBody] string password)
        {
            string? contextUserId = HttpContext.GetUserId();
            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER) && contextUserId != userId)
            {
                return Unauthorized();
            }

            var user = await _userService.UpdatePasswordAsync(userId, password);

            if (user == null)
            {
                return NotFound(userId);
            }

            return AcceptedAtAction(nameof(FindOne), new { id = user.Id }, OffendUserFields(user));
        }

        [HttpGet("{userId}/activation")]
        public async Task<ActionResult<User>> ActivateUser(string userId, [FromQuery] string activationCode)
        {
            var user = await _userService.FindOneAsync(userId);
            if (user == null)
            {
                return NotFound(userId);
            }

            user = await _userService.ActivateUserAsync(user, activationCode);
            if (user == null)
            {
                return NotFound(userId);
            }

            return Ok(OffendUserFields(user));
        }

        private User OffendUserFields(User user)
        {
            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER))
            {
                return user with
                {
                    ActivationCode = null,
                    ActivationValidity = null,
                    PasswordHash = null
                };
            }
            return user;
        }
    }
}
