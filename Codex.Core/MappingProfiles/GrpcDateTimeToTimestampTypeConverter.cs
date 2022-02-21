using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace Codex.Core.MappingProfiles;

public class GrpcDateTimeToTimestampTypeConverter : ITypeConverter<DateTime, Timestamp>
{
    public Timestamp Convert(DateTime source, Timestamp? destination, ResolutionContext context)
    {/*
        if (source == null)
        {
            return new Timestamp();
        }*/

        return Timestamp.FromDateTime(DateTime.SpecifyKind(source, DateTimeKind.Utc));
    }
    
    /*
        CreateMap<DateTime, Timestamp>().ConvertUsing(x => Timestamp.FromDateTime(DateTime.SpecifyKind(x, DateTimeKind.Utc)));
        CreateMap<Timestamp, DateTime>().ConvertUsing(x => x.ToDateTime());*/
}

public class GrpcTimestampTypeToDateTimeConverter : ITypeConverter<Timestamp, DateTime>
{
    public DateTime Convert(Timestamp source, DateTime destination, ResolutionContext context)
    {
        return source?.ToDateTime() ?? DateTime.MinValue;
    }
}