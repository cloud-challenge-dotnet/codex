using AutoMapper;
using Codex.Models.Tenants;
using CodexGrpc.Tenants;

namespace Codex.Tenants.Api.MappingProfiles;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        //Project GRPC mapping
        CreateMap<TenantModel, Tenant>().ReverseMap();
    }
}