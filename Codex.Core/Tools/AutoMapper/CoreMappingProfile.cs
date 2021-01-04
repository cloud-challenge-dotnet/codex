using AutoMapper;
using Codex.Core.Repositories;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace Codex.Core.Tools.AutoMapper
{
    public class CoreMappingProfile : Profile
    {
        public CoreMappingProfile()
        {
            CreateMap<List<ObjectId>, List<string>>().ConvertUsing(o => o.Select(os => os.ToString()).ToList());
            CreateMap<List<string>, List<ObjectId>>().ConvertUsing(o => o.Select(os => ObjectId.Parse(os)).ToList());
            CreateMap<ObjectId, string>().ConvertUsing(o => o.ToString());
            CreateMap<string, ObjectId>().ConvertUsing(s => ObjectId.Parse(s));

            CreateMap<string, TranslationDataRow>().ConvertUsing<StringToTranslationDataRowConverter>();
            CreateMap<TranslationDataRow, string>().ConvertUsing<TranslationDataRowToStringConverter>();
        }
    }
}
