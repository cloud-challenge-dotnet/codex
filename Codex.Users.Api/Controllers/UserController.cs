using Codex.Models.Users;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<User>> FindOne(string id)
        {
            var user = await _userService.FindOneAsync(id);

            return user == null ? NotFound(id) : Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<IEnumerable<User>>> FindAll([FromQuery] UserCriteria userCriteria)
        {
            var users = await _userService.FindAllAsync(userCriteria);

            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserCreator userCreator)
        {
            var user = await _userService.CreateAsync(userCreator);

            return CreatedAtAction(nameof(FindOne), new { id = user.Id }, user);
        }
    }
}
