using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;

namespace Codex.Models.Users
{
    public abstract record BaseUser
    {
        public BaseUser()
               => (Login, Email, FirstName, LastName, PhoneNumber, Roles, CreationDate, ModificationDate) = ("", "", null, null, null, new(), DateTime.Now, DateTime.Now);

        public BaseUser(string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles)
               => (Login, Email, FirstName, LastName, PhoneNumber, Roles, CreationDate, ModificationDate) = (login, email, firstName, lastName, phoneNumber, roles, DateTime.Now, DateTime.Now);

        public string Login { get; set; }

        public string Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public List<string> Roles { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ModificationDate { get; set; }
    }

    public record User : BaseUser
    {
        public User() : base()
            => (Id, PasswordHash, ActivationValidity, Active) = (null, null, null, true);

        public User(ObjectId? id, string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles,
            string? passwordHash, string? activationCode = null, DateTime? activationValidity = null, bool active = true)
            : base(login, email, firstName, lastName, phoneNumber, roles)
            => (Id, PasswordHash, ActivationCode, ActivationValidity, Active) = (id, passwordHash, activationCode, activationValidity, active);

        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? Id { get; set; }

        public string? PasswordHash { get; set; }

        public string? ActivationCode { get; set; }

        public DateTime? ActivationValidity { get; set; }

        public bool Active { get; set; }
    }

    public record UserCreator : BaseUser
    {
        public UserCreator() : base()
            => (Password) = (null);

        public string? Password { get; set; }

        public User ToUser(string? passwordHash = null) => new User(
            id: null,
            login: Login,
            email: Email,
            firstName: FirstName,
            lastName: LastName,
            phoneNumber: PhoneNumber,
            roles: Roles,
            active: true,
            passwordHash: passwordHash
        );
    }
}
