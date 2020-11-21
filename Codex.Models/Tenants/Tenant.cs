using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Codex.Models.Tenants
{
    public abstract record BaseTenant
    {
        public BaseTenant()
            => (Id, Name, Properties) = (null, "", null);

        public BaseTenant(string? id, string name)
           => (Id, Name, Properties) = (id, name, null);

        public BaseTenant(string? id, string name, TenantProperties? properties)
           => (Id, Name, Properties) = (id, name, properties);

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string? Id { get; set; }

        public string Name { get; set; }

        public TenantProperties? Properties { get; set; }
    }

    public record Tenant : BaseTenant
    {
        public Tenant() : base()
        {
        }

        public Tenant(string? id, string name) : base(id, name)
        {
        }

        public Tenant(string? id, string name, TenantProperties? properties) : base(id, name, properties)
        {
        }
    }

    public record TenantCreator : BaseTenant
    {
        public TenantCreator() : base()
        {
        }

        public TenantCreator(string? id, string name) : base(id, name)
        {
        }

        public TenantCreator(string? id, string name, TenantProperties properties) : base(id, name, properties)
        {
        }

        public Tenant ToTenant() => new Tenant(id: Id, name: Name, properties: Properties);
    }

    [Serializable]
    public class TenantProperties : Dictionary<string, List<string>>
    {
    }
}
