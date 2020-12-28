using AutoMapper;
using Codex.Models.Tenants;
using Codex.Tenants.Api.Repositories.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace Codex.Tenants.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<List<ObjectId>, List<string>>().ConvertUsing(o => o.Select(os => os.ToString()).ToList());
            CreateMap<List<string>, List<ObjectId>>().ConvertUsing(o => o.Select(os => ObjectId.Parse(os)).ToList());
            CreateMap<ObjectId, string>().ConvertUsing(o => o.ToString());
            CreateMap<string, ObjectId>().ConvertUsing(s => ObjectId.Parse(s));
            CreateMap<Tenant, TenantRow>().ReverseMap();
        }
    }
}
