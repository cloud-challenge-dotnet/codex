using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Codex.Tenants.Api.Repositories.Models
{
    public record TenantRow
    {
        public TenantRow(string? id = null, string name = "", Dictionary<string, List<string>>? properties = null)
           => (Id, Name, Properties) = (id, name, properties);

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string? Id { get; init; }

        public string Name { get; init; }

        public Dictionary<string, List<string>>? Properties { get; init; }
    }
}
