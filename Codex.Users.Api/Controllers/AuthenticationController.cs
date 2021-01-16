using Codex.Models.Users;
using Codex.Users.Api.Resources;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        private readonly IStringLocalizer<UserResource> _sl;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IStringLocalizer<UserResource> sl)
        {
            _authenticationService = authenticationService;
            _sl = sl;
        }

        [HttpPost]
        public Task<ActionResult<Auth>> Authenticate([FromHeader] string tenantId, [FromBody] UserLogin userLogin)
        {
            if (tenantId != userLogin.TenantId)
            {
                throw new ArgumentException(_sl[UserResource.TENANT_ID_INSIDE_HEADER_MUST_BE_SAME_THAN_USER_TENANT_ID]!);
            }

            return AuthenticateInternalAsync(userLogin);
        }

        public async Task<ActionResult<Auth>> AuthenticateInternalAsync([FromBody] UserLogin userLogin)
        {
            Auth auth = await _authenticationService.AuthenticateAsync(userLogin);

            return new OkObjectResult(auth);
        }
    }
}
