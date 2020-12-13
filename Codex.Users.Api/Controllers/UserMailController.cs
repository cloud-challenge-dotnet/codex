using Codex.Models.Users;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserMailController : ControllerBase
    {
        private readonly IUserMailService _userMailService;

        public UserMailController(IUserMailService userMailService)
        {
            _userMailService = userMailService;
        }

        [HttpPost("activation")]
        //[Authorize(Roles = RoleConstant.ADMIN)] // TODO add role
        public async Task<ActionResult> SendActivateUserMail([FromHeader]string tenantId, [FromBody] User user)
        {
            await _userMailService.SendActivateUserMailAsync(tenantId, user);

            return Ok();
        }
    }
}
