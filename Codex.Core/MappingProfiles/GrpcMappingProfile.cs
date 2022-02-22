using System;
using System.Collections.Generic;
using AutoMapper;
using CodexGrpc.Common;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Codex.Core.MappingProfiles;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        //Common GRPC mapping
        CreateMap(typeof(IEnumerable<>), typeof(RepeatedField<>)).ConvertUsing(typeof(GrpcEnumerableToRepeatedFieldTypeConverter<,>));
        CreateMap(typeof(RepeatedField<>), typeof(List<>)).ConvertUsing(typeof(GrpcRepeatedFieldToListTypeConverter<,>));
        CreateMap(typeof(Dictionary<string, List<string>>), typeof(MapField<string, StringValues>)).ConvertUsing(typeof(GrpcDictionaryToMapFieldTypeConverter));
        CreateMap(typeof(MapField<string, StringValues>), typeof(Dictionary<string, List<string>>)).ConvertUsing(typeof(GrpcMapFieldToDictionaryTypeConverter));
        CreateMap(typeof(Timestamp), typeof(DateTime)).ConvertUsing(typeof(GrpcTimestampTypeToDateTimeConverter));
        CreateMap(typeof(DateTime), typeof(Timestamp)).ConvertUsing(typeof(GrpcDateTimeToTimestampTypeConverter));
    }
}