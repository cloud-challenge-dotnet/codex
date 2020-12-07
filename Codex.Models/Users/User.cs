using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Codex.Models.Users
{
    public abstract record BaseUser
    {
        public BaseUser()
               => (Login, Email, FirstName, LastName, PhoneNumber, Roles) = ("", "", null, null, null, new());

        public BaseUser(string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles)
               => (Login, Email, FirstName, LastName, PhoneNumber, Roles) = (login, email, firstName, lastName, phoneNumber, roles);

        public string Login { get; set; }

        public string Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public List<string> Roles { get; set; }
    }

    public record User : BaseUser
    {
        public User() : base()
            => (Id, PasswordHash, EmailConfirmed, PhoneConfirmed, Active) = (null, null, false, false, true);

        public User(string? id, string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles,
            string? passwordHash, bool emailConfirmed, bool phoneConfirmed, bool active)
            : base(login, email, firstName, lastName, phoneNumber, roles)
            => (Id, PasswordHash, EmailConfirmed, PhoneConfirmed, Active) = (id, passwordHash, emailConfirmed, phoneConfirmed, active);

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? PasswordHash { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool PhoneConfirmed { get; set; }

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
            passwordHash: passwordHash,
            emailConfirmed: false,
            phoneConfirmed: false
        );
    }
}
