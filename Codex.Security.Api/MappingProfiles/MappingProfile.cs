using AutoMapper;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Models;

namespace Codex.Security.Api.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApiKey, ApiKeyRow>().ReverseMap();
    }
}