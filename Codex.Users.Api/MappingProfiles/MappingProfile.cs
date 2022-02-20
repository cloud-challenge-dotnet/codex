using AutoMapper;
using Codex.Models.Users;
using Codex.Users.Api.Repositories.Models;

namespace Codex.Users.Api.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserRow>().ReverseMap();
    }
}