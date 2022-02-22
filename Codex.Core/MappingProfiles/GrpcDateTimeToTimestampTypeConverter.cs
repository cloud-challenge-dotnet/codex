using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
// ReSharper disable ConstantConditionalAccessQualifier

namespace Codex.Core.MappingProfiles;

public class GrpcDateTimeToTimestampTypeConverter : ITypeConverter<DateTime, Timestamp>
{
    public Timestamp Convert(DateTime source, Timestamp? destination, ResolutionContext context)
    {
        return Timestamp.FromDateTime(DateTime.SpecifyKind(source, DateTimeKind.Utc));
    }
}

public class GrpcTimestampTypeToDateTimeConverter : ITypeConverter<Timestamp, DateTime>
{
    public DateTime Convert(Timestamp source, DateTime destination, ResolutionContext context)
    {
        return source?.ToDateTime() ?? DateTime.MinValue;
    }
}