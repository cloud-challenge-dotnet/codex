using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;

namespace Codex.Users.Api.Repositories.Models
{
    public record UserRow
    {
        public UserRow()
               => (Id, Login, Email, FirstName, LastName, PhoneNumber, Roles, CreationDate, ModificationDate, LanguageCultureName) =
                  (null, "", "", null, null, null, new(), DateTime.Now, DateTime.Now, "en-US");

        public UserRow(ObjectId? id, string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles, string languageCultureName)
               => (Id, Login, Email, FirstName, LastName, PhoneNumber, Roles, LanguageCultureName, CreationDate, ModificationDate) =
                  (id, login, email, firstName, lastName, phoneNumber, roles, languageCultureName, DateTime.Now, DateTime.Now);

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? Id { get; init; }

        public string Login { get; init; }

        public string Email { get; init; }

        public string? FirstName { get; init; }

        public string? LastName { get; init; }

        public string? PhoneNumber { get; init; }

        public List<string> Roles { get; init; }

        public string LanguageCultureName { get; init; }

        public DateTime CreationDate { get; init; }

        public DateTime ModificationDate { get; init; }

        public string? PasswordHash { get; init; }

        public string? ActivationCode { get; init; }

        public DateTime? ActivationValidity { get; init; }

        public bool Active { get; init; }
    }
}
