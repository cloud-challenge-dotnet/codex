using Codex.Core.Exceptions;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Tools;
using Codex.Models.Users;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        public UserService(IUserRepository userRepository, DaprClient daprClient, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _daprClient = daprClient;
            _passwordHasher = passwordHasher;
        }

        private readonly IUserRepository _userRepository;
        private readonly DaprClient _daprClient;
        private readonly IPasswordHasher _passwordHasher;

        public async Task<List<User>> FindAllAsync(UserCriteria userCriteria)
        {
            return await _userRepository.FindAllAsync(userCriteria);
        }

        public async Task<User?> FindOneAsync(string id)
        {
            return await _userRepository.FindOneAsync(id);
        }

        public async Task<User> CreateAsync(UserCreator userCreator)
        {
            if (string.IsNullOrWhiteSpace(userCreator.Password))
                throw new IllegalArgumentException(code: "USER_PASSWORD_INVALID", message: "Password must be not null or whitespace");

            if (string.IsNullOrWhiteSpace(userCreator.Login))
                throw new IllegalArgumentException(code: "USER_LOGIN_INVALID", message: "Login must be not null or whitespace");

            if (string.IsNullOrWhiteSpace(userCreator.Email) || !EmailValidator.EmailValid(userCreator.Email))
                throw new IllegalArgumentException(code: "USER_EMAIL_INVALID", message: "Email format is invalid");

            if ((await _userRepository.FindAllAsync(new (Login: userCreator.Login))).Count != 0 ||
                (await _userRepository.FindAllAsync(new(Email: userCreator.Email))).Count != 0)
            {
                throw new IllegalArgumentException(code: "USER_EXISTS", message: $"User '{userCreator.Login}' already exists");
            }

            var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.PasswordSalt);
            var salt = secretValues[ConfigConstant.PasswordSalt];

            var user = userCreator.ToUser(passwordHash: _passwordHasher.GenerateHash(userCreator.Password!, salt));

            return await _userRepository.InsertAsync(user);

            // TODO send mail to user for verify his email
        }

        public async Task<User?> UpdateAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);
        }
    }
}
