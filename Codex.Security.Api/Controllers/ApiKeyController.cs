using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Models.Security;
using Codex.Security.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Security.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpGet("{id}")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<ApiKey>> FindOne(string id)
        {
            var apiKey = await _apiKeyService.FindOneAsync(id);

            return apiKey == null ? NotFound(id) : Ok(apiKey);
        }

        [HttpGet]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<IEnumerable<ApiKey>>> FindAll([FromQuery] ApiKeyCriteria apiKeyCriteria)
        {
            var apiKeyList = await _apiKeyService.FindAllAsync(apiKeyCriteria);

            return Ok(apiKeyList);
        }

        [HttpPost]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<ApiKey>> CreateApiKey([FromBody] ApiKey apiKey)
        {
            apiKey = await _apiKeyService.CreateAsync(apiKey);

            return base.CreatedAtAction(nameof(FindOne), new { id = apiKey.Id }, apiKey);
        }

        [HttpPut("{apiKeyId}")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<ApiKey>> UpdateApiKey(string apiKeyId, [FromBody] ApiKey apiKey)
        {
            apiKey = apiKey with { Id = apiKeyId };

            var apiKeyResult = await _apiKeyService.UpdateAsync(apiKey);
            if (apiKeyResult == null)
            {
                return NotFound(apiKeyId);
            }

            return AcceptedAtAction(nameof(FindOne), new { id = apiKey.Id }, apiKeyResult);
        }

        [HttpDelete("{apiKeyId}")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult> DeleteApiKey(string apiKeyId)
        {
            await _apiKeyService.DeleteAsync(apiKeyId);

            return NoContent();
        }
    }
}
