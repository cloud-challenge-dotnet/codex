using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Codex.Models.Users
{
    public abstract record BaseUser
    {
        public BaseUser()
               => (Id, Login, Email, FirstName, LastName, PhoneNumber, Roles) = (null, "", "", null, null, null, new());

        public BaseUser(string? id, string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles)
               => (Id, Login, Email, FirstName, LastName, PhoneNumber, Roles) = (id, login, email, firstName, lastName, phoneNumber, roles);

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

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
            => (PasswordHash, EmailConfirmed, PhoneConfirmed, Active) = (null, false, false, true);

        public User(string? id, string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles,
            string? passwordHash, bool emailConfirmed, bool phoneConfirmed, bool active)
            : base(id, login, email, firstName, lastName, phoneNumber, roles)
            => (PasswordHash, EmailConfirmed, PhoneConfirmed, Active) = (passwordHash, emailConfirmed, phoneConfirmed, active);

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
            id: Id,
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
