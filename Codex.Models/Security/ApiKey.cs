using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Codex.Models.Security
{
    public record ApiKey
    {
        public ApiKey() : base()
            => (Id, Name, Roles) = (null, "", new());

        public ApiKey(string? id, string name, List<string> roles)
            => (Id, Name, Roles) = (id, name, roles);

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string? Id { get; set; }

        public string Name { get; set; }

        public List<string> Roles { get; set; }
    }
}
