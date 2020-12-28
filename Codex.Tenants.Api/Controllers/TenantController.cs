using Codex.Core.Models;
using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Services;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.Tenants.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TenantController : ControllerBase
    {
        public TenantController(
            ITenantService tenantService,
            ITenantPropertiesService tenantPropertiesService,
            DaprClient daprClient)
        {
            _tenantService = tenantService;
            _tenantPropertiesService = tenantPropertiesService;
            _daprClient = daprClient;
        }

        private readonly DaprClient _daprClient;
        private readonly ITenantService _tenantService;
        private readonly ITenantPropertiesService _tenantPropertiesService;

        [HttpGet("{id}")]
        public async Task<ActionResult<Tenant>> FindOne(string id)
        {
            var tenant = await _tenantService.FindOneAsync(id);

            if (tenant == null)
                return NotFound(id);

            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER))
            {
                tenant = tenant with { Properties = null };
            }

            return Ok(tenant);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tenant>>> FindAll()
        {
            var tenantList = (await _tenantService.FindAllAsync());

            if (!HttpContext.User.IsInRole(RoleConstant.TENANT_MANAGER))
            {
                tenantList = tenantList.Select(t => t with { Properties = null }).ToList();
            }

            return Ok(tenantList);
        }

        [HttpPost]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<Tenant>> CreateTenant([FromBody] Tenant tenant)
        {
            tenant = await _tenantService.CreateAsync(tenant);

            await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

            return CreatedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<Tenant>> UpdateTenant(string tenantId, [FromBody] Tenant tenant)
        {
            var tenantResult = await _tenantService.UpdateAsync(tenant with { Id = tenantId });
            if (tenantResult == null)
            {
                return NotFound(tenantId);
            }

            await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenantResult);
        }

        [HttpPut("{tenantId}/properties")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<Tenant>> UpdateProperties(string tenantId, [FromBody] TenantProperties tenantProperties)
        {
            var tenant = await _tenantPropertiesService.UpdatePropertiesAsync(tenantId, tenantProperties);
            if (tenant == null)
            {
                return NotFound(tenantId);
            }

            await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpPut("{tenantId}/properties/{propertyKey}")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<Tenant>> UpdateProperty(string tenantId, [FromQuery] string propertyKey, [FromBody] List<string> values)
        {
            var tenant = await _tenantPropertiesService.UpdatePropertyAsync(tenantId, propertyKey, values);
            if (tenant == null)
            {
                return NotFound(tenantId);
            }

            await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        [HttpGet("{tenantId}/properties")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<Dictionary<string, List<string>>?>> FindProperties(string tenantId)
        {
            var tenantProperties = await _tenantPropertiesService.FindPropertiesAsync(tenantId);

            return Ok(tenantProperties);
        }

        [HttpDelete("{tenantId}/properties/{propertyKey}")]
        [TenantAuthorize(Roles = RoleConstant.TENANT_MANAGER)]
        public async Task<ActionResult<Tenant>> DeleteProperty(string tenantId, [FromQuery] string propertyKey)
        {
            var tenant = await _tenantPropertiesService.DeletePropertyAsync(tenantId, propertyKey);

            if (tenant == null)
            {
                return NoContent();
            }

            await PublishTenantChangeEventAsync(TopicType.Modify, tenant);

            return AcceptedAtAction(nameof(FindOne), new { id = tenant.Id }, tenant);
        }

        private async Task PublishTenantChangeEventAsync(TopicType topicType, Tenant tenant)
        {
            await _daprClient.PublishEventAsync(ConfigConstant.CodexPubSubName, TopicConstant.Tenant, new TopicData<Tenant>(topicType, tenant, tenant.Id!));
        }
    }
}
