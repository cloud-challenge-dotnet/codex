using Codex.Tenants.Api.Services;
using Codex.Models.Tenants;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Codex.Tenants.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TenantController : ControllerBase
    {
        public TenantController(
            ITenantService tenantService,
            ITenantPropertiesService tenantPropertiesService)
        {
            _tenantService = tenantService;
            _tenantPropertiesService = tenantPropertiesService;
        }

        private readonly ITenantService _tenantService;
        private readonly ITenantPropertiesService _tenantPropertiesService;

        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> FindOne(string id)
        {
            var tenant = await _tenantService.FindOneAsync(id);

            return tenant == null ? NotFound(id) : Ok(tenant);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> FindAll()
        {
            var tenantList = (await _tenantService.FindAllAsync())
                .Select(t => t with { Properties = null }).ToList();

            return Ok(tenantList);
        }

        [HttpPost]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<Tenant>> CreateTenant([FromBody] TenantCreator tenantCreator)
        {
            var tenant = await _tenantService.CreateAsync(tenantCreator);
            return CreatedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}")]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<Tenant>> UpdateTenant([FromQuery]string tenantId, [FromBody] Tenant tenant)
        {
            var tenantResult = await _tenantService.UpdateAsync(tenant with { Id = tenantId });
            if (tenantResult == null)
            {
                return NotFound(tenantId);
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenantResult);
        }

        [HttpPut("{tenantId}/properties")]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<Tenant>> UpdateProperties([FromQuery] string tenantId, [FromBody] TenantProperties tenantProperties)
        {
            var tenant = await _tenantPropertiesService.UpdatePropertiesAsync(tenantId, tenantProperties);
            if (tenant == null)
            {
                return NotFound(tenantId);
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}/properties/{propertyKey}")]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<Tenant>> UpdateProperty([FromQuery] string tenantId, [FromQuery] string propertyKey, [FromBody] List<string> values)
        {
            var tenant = await _tenantPropertiesService.UpdatePropertyAsync(tenantId, propertyKey, values);
            if (tenant == null)
            {
                return NotFound(tenantId);
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpGet("{tenantId}/properties")]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<TenantProperties?>> FindProperties([FromQuery] string tenantId)
        {
            var tenantProperties = await _tenantPropertiesService.FindPropertiesAsync(tenantId);

            return Ok(tenantProperties);
        }

        [HttpDelete("{tenantId}/properties/{propertyKey}")]
        [Authorize(Roles = "TENANT_MANAGER")]
        public async Task<ActionResult<Tenant>> DeleteProperty([FromQuery] string tenantId, [FromQuery] string propertyKey)
        {
            var tenant = await _tenantPropertiesService.DeletePropertyAsync(tenantId, propertyKey);

            if (tenant == null)
            {
                return NoContent();
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }
    }
}
