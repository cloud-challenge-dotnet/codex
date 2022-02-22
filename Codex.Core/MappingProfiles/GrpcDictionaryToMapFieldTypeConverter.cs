using System.Collections.Generic;
using AutoMapper;
using CodexGrpc.Common;
using Google.Protobuf.Collections;

namespace Codex.Core.MappingProfiles;

public class GrpcDictionaryToMapFieldTypeConverter : ITypeConverter<IDictionary<string, List<string>>, MapField<string, StringValues>>
{
    public MapField<string, StringValues> Convert(IDictionary<string, List<string>>? source, MapField<string, StringValues>? destination, ResolutionContext context)
    {
        destination ??= new MapField<string, StringValues>();

        if (source != null)
        {
            foreach (var keyVal in source)
            {
                var stringValues = new StringValues();
                stringValues.Values.AddRange(context.Mapper.Map<RepeatedField<string>>(keyVal.Value));
                destination.Add(keyVal.Key, stringValues);
            }
        }

        return destination;
    }
}

public class GrpcMapFieldToDictionaryTypeConverter : ITypeConverter<MapField<string, StringValues>, Dictionary<string, List<string>>>
{
    public Dictionary<string, List<string>> Convert(MapField<string, StringValues>? source, Dictionary<string, List<string>>? destination, ResolutionContext context)
    {
        destination ??= new Dictionary<string, List<string>>();

        if (source != null)
        {
            foreach (var keyVal in source)
            {
                var values = new List<string>();
                values.AddRange(context.Mapper.Map<List<string>>(keyVal.Value.Values));
                destination.Add(keyVal.Key, values);
            }
        }

        return destination;
    }
}