using Codex.Models.Users;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        public async Task<ActionResult<Auth>> Authenticate([FromHeader]string tenantId, [FromBody] UserLogin userLogin)
        {
            if(tenantId != userLogin.TenantId)
            {
                throw new ArgumentException("Tenant id inside header must be same than user tenant id");
            }

            Auth auth = await _authenticationService.AuthenticateAsync(userLogin);

            return new OkObjectResult(auth);
        }
    }
}
