using System;
using System.Collections.Generic;

namespace Codex.Models.Users
{
    public abstract record BaseUser
    {
        public BaseUser()
               => (Login, Email, FirstName, LastName, PhoneNumber, Roles, CreationDate, ModificationDate, LanguageCultureName) =
                  ("", "", null, null, null, new(), DateTime.Now, DateTime.Now, "en-US");

        public BaseUser(string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles, string languageCultureName = "en-US")
               => (Login, Email, FirstName, LastName, PhoneNumber, Roles, LanguageCultureName, CreationDate, ModificationDate) =
                  (login, email, firstName, lastName, phoneNumber, roles, languageCultureName, DateTime.Now, DateTime.Now);

        public string Login { get; init; }

        public string Email { get; init; }

        public string? FirstName { get; init; }

        public string? LastName { get; init; }

        public string? PhoneNumber { get; init; }

        public List<string> Roles { get; init; }

        public string LanguageCultureName { get; init; }

        public DateTime CreationDate { get; init; }

        public DateTime ModificationDate { get; init; }
    }

    public record User : BaseUser
    {
        public User() : base()
            => (Id, PasswordHash, ActivationValidity, Active) = (null, null, null, true);

        public User(string? id, string login, string email, string? firstName, string? lastName, string? phoneNumber, List<string> roles,
            string? passwordHash, string? activationCode = null, DateTime? activationValidity = null, bool active = true, string languageCultureName="en")
            : base(login, email, firstName, lastName, phoneNumber, roles, languageCultureName)
            => (Id, PasswordHash, ActivationCode, ActivationValidity, Active) = (id, passwordHash, activationCode, activationValidity, active);

        public string? Id { get; init; }

        public string? PasswordHash { get; init; }

        public string? ActivationCode { get; init; }

        public DateTime? ActivationValidity { get; init; }

        public bool Active { get; init; }
    }

    public record UserCreator : BaseUser
    {
        public UserCreator() : base()
            => (Password) = (null);

        public string? Password { get; init; }

        public User ToUser(string? passwordHash = null) => new User(
            id: null,
            login: Login,
            email: Email,
            firstName: FirstName,
            lastName: LastName,
            phoneNumber: PhoneNumber,
            roles: Roles,
            languageCultureName: LanguageCultureName,
            active: true,
            passwordHash: passwordHash
        );
    }
}
