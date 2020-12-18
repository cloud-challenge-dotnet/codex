

using Codex.Core.Exceptions;
using MongoDB.Bson;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Codex.Core.Tools
{
    /*
     * BSON ObjectID is a 12-byte value consisting of:
     * - a 4-byte timestamp (seconds since epoch)
     * - a 3-byte machine id
     * - a 2-byte process id
     * - a 3-byte counter
     * 
     * 0123 456     78  91011
     * time machine pid inc
     */
    [ExcludeFromCodeCoverage]
    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(ObjectId);
        }

        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new IllegalArgumentException(
                    String.Format("Unexpected token parsing ObjectId. Expected String, got {0}.",
                                  reader.TokenType));
            }

            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? ObjectId.Empty : new ObjectId(value);
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            if (value is ObjectId objectId)
            {
                string data = objectId != ObjectId.Empty ? objectId.ToString() : string.Empty;
                writer.WriteStringValue(data);
            }
            else
            {
                throw new IllegalArgumentException("Expected ObjectId value.");
            }
        }
    }
}
