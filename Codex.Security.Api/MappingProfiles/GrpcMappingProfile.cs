using AutoMapper;
using Codex.Models.Security;
using Codex.Models.Tenants;
using CodexGrpc.Security;
using CodexGrpc.Tenants;

namespace Codex.Security.Api.MappingProfiles;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        //Project GRPC mapping
        CreateMap<TenantModel, Tenant>().ReverseMap();
        CreateMap<ApiKeyModel, ApiKey>().ReverseMap();
        CreateMap<ApiKeyCriteriaModel, ApiKeyCriteria>().ReverseMap();
    }
}