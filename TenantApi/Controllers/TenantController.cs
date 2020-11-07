using Codex.Tenants.Api.Services;
using Codex.Tenants.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Controllers
{
    // TODO add security layer !!!!!!
    [ApiController]
    [Route("[controller]")]
    public class TenantController : ControllerBase
    {
        public TenantController(
            TenantService tenantService,
            TenantPropertiesService tenantPropertiesService)
        {
            _tenantService = tenantService;
            _tenantPropertiesService = tenantPropertiesService;
        }

        private readonly TenantService _tenantService;
        private readonly TenantPropertiesService _tenantPropertiesService;

        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> FindOne(string id)
        {
            var tenant = await _tenantService.FindOneAsync(id);

            return tenant == null ? NotFound() : Ok(tenant);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> FindAll()
        {
            var tenantList = await _tenantService.FindAllAsync();

            return tenantList.Select(t => t with { Key = null, Properties = null }).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<Tenant>> CreateTenant([FromBody] TenantCreator tenantCreator)
        {
            var tenant = await _tenantService.CreateAsync(tenantCreator);
            return CreatedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}")]
        public async Task<ActionResult<Tenant>> UpdateTenant([FromQuery]string tenantId, [FromBody] Tenant tenant)
        {
            tenant = await _tenantService.UpdateAsync(tenant with { Id = tenantId });
            if(tenant == null)
            {
                return NotFound();
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}/properties")]
        public async Task<ActionResult<Tenant>> UpdateProperties([FromQuery] string tenantId, [FromBody] TenantProperties tenantProperties)
        {
            var tenant = await _tenantPropertiesService.UpdatePropertiesAsync(tenantId, tenantProperties);
            if (tenant == null)
            {
                return NotFound();
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}/properties/{propertyKey}")]
        public async Task<ActionResult<Tenant>> UpdateProperty([FromQuery] string tenantId, [FromQuery] string propertyKey, [FromBody] List<string> values)
        {
            var tenant = await _tenantPropertiesService.UpdatePropertyAsync(tenantId, propertyKey, values);
            if (tenant == null)
            {
                return NotFound();
            }
            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpGet("{tenantId}/properties")]
        public async Task<ActionResult<TenantProperties?>> FindProperties([FromQuery] string tenantId)
        {
            var tenantProperties = await _tenantPropertiesService.FindPropertiesAsync(tenantId);

            return Ok(tenantProperties);
        }

        [HttpDelete("{tenantId}/properties/{propertyKey}")]
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
