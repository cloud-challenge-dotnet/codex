using System.Collections.Generic;
using AutoMapper;
using Google.Protobuf.Collections;

namespace Codex.Core.MappingProfiles;

public class GrpcEnumerableToRepeatedFieldTypeConverter<TITemSource, TITemDest> : ITypeConverter<IEnumerable<TITemSource>, RepeatedField<TITemDest>>
{
    public RepeatedField<TITemDest> Convert(IEnumerable<TITemSource> source, RepeatedField<TITemDest>? destination, ResolutionContext context)
    {
        destination = destination ?? new RepeatedField<TITemDest>();
        foreach (var item in source)
        {
            // obviously we haven't performed the mapping for the item yet
            // since AutoMapper didn't recognise the list conversion
            // so we need to map the item here and then add it to the new
            // collection
            destination.Add(context.Mapper.Map<TITemDest>(item));
        }
        return destination;
    }
}

public class GrpcRepeatedFieldToListTypeConverter<TITemSource, TITemDest> : ITypeConverter<RepeatedField<TITemSource>, List<TITemDest>>
{
    public List<TITemDest> Convert(RepeatedField<TITemSource> source, List<TITemDest>? destination, ResolutionContext context)
    {
        destination = destination ?? new List<TITemDest>();
        foreach (var item in source)
        {
            destination.Add(context.Mapper.Map<TITemDest>(item));
        }
        return destination;
    }
}