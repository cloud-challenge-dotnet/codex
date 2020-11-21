using Codex.Core.Interfaces;
using Codex.Models.Users;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly DaprClient _daprClient;

        public UserService(IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            DaprClient daprClient)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _daprClient = daprClient;
        }

        private readonly IUserRepository _userRepository;

        public async Task<List<User>> FindAllAsync(UserCriteria userCriteria)
        {
            return await _userRepository.FindAllAsync(userCriteria);
        }

        public async Task<User?> FindOneAsync(string id)
        {
            return await _userRepository.FindOneAsync(id);
        }

        public Task<User> CreateAsync(UserCreator userCreator)
        {
            if (string.IsNullOrWhiteSpace(userCreator.Password))
                throw new ArgumentException("Password must be not null or whitespace");

            return CreateInternalAsync(userCreator);
        }

        public async Task<User> CreateInternalAsync(UserCreator userCreator)
        {
            var secretValues = await _daprClient.GetSecretAsync("codex", "passwordSalt");
            var salt = secretValues["passwordSalt"];

            var user = userCreator.ToUser(passwordHash: _passwordHasher.GenerateHash(userCreator.Password!, salt));

            return await _userRepository.InsertAsync(user);
        }


        public async Task<User?> UpdateAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);
        }
    }
}
