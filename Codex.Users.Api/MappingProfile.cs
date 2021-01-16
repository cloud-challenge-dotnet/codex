using AutoMapper;
using Codex.Models.Users;
using Codex.Users.Api.Repositories.Models;

namespace Codex.Users.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserRow>().ReverseMap();
        }
    }
}
