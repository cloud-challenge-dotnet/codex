using AutoMapper;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Repositories.Models;

namespace Codex.Tenants.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Tenant, TenantRow>().ReverseMap();
        }
    }
}
