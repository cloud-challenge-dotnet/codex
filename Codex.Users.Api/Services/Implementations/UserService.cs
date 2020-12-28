using AutoMapper;
using Codex.Core;
using Codex.Core.Extensions;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Tools;
using Codex.Models.Exceptions;
using Codex.Models.Users;
using Codex.Users.Api.Exceptions;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Repositories.Models;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.Users.Api.Services.Implementations
{
    public class UserService : IUserService
    {
        public UserService(
            IUserRepository userRepository,
            DaprClient daprClient,
            IPasswordHasher passwordHasher,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _daprClient = daprClient;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        private readonly IUserRepository _userRepository;
        private readonly DaprClient _daprClient;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public async Task<List<User>> FindAllAsync(UserCriteria userCriteria)
        {
            var userRows = await _userRepository.FindAllAsync(userCriteria);

            return userRows.Select(it => _mapper.Map<User>(it)).ToList();
        }

        public async Task<User?> FindOneAsync(string id)
        {
            var userRow = await _userRepository.FindOneAsync(new ObjectId(id));
            return userRow?.Let(it => _mapper.Map<User>(it));
        }

        public async Task<User> CreateAsync(string tenantId, UserCreator userCreator)
        {
            if (string.IsNullOrWhiteSpace(userCreator.Password))
                throw new IllegalArgumentException(code: "USER_PASSWORD_INVALID", message: "Password must be not null or whitespace");

            if (string.IsNullOrWhiteSpace(userCreator.Login))
                throw new IllegalArgumentException(code: "USER_LOGIN_INVALID", message: "Login must be not null or whitespace");

            if (string.IsNullOrWhiteSpace(userCreator.Email) || !EmailValidator.EmailValid(userCreator.Email))
                throw new IllegalArgumentException(code: "USER_EMAIL_INVALID", message: "Email format is invalid");

            if ((await _userRepository.FindAllAsync(new(Login: userCreator.Login))).Count != 0 ||
                (await _userRepository.FindAllAsync(new(Email: userCreator.Email))).Count != 0)
            {
                throw new IllegalArgumentException(code: "USER_EXISTS", message: $"User '{userCreator.Login}' already exists");
            }

            var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.PasswordSalt);
            var salt = secretValues[ConfigConstant.PasswordSalt];

            var user = userCreator.ToUser(passwordHash: _passwordHasher.GenerateHash(userCreator.Password!, salt));

            // generate activation code for 30 days
            user = user with { ActivationCode = StringUtils.RandomString(50), ActivationValidity = DateTime.Now.AddDays(30) };

            var userRow = await _userRepository.InsertAsync(_mapper.Map<UserRow>(user));

            user = _mapper.Map<User>(userRow);

            await SendActivationUserEmailAsync(user, tenantId);

            return user;
        }

        public async Task<User?> UpdateAsync(User user)
        {
            var userRow = await _userRepository.UpdateAsync(_mapper.Map<UserRow>(user));

            return userRow?.Let(it => _mapper.Map<User>(it));
        }

        private async Task SendActivationUserEmailAsync(User user, string tenantId)
        {
            await _daprClient.PublishEventAsync(ConfigConstant.CodexPubSubName, TopicConstant.SendActivationUserMail, new TopicData<User>(TopicType.Modify, user, tenantId));
        }

        public async Task<User?> ActivateUserAsync(User user, string activationCode)
        {
            if (user.ActivationCode == null || user.ActivationCode != activationCode)
            {
                throw new InvalidUserValidationCodeException("Validation code is invalid", code: "INVALID_VALIDATION_CODE");
            }

            if (user.ActivationValidity == null || DateTime.Now > user.ActivationValidity!)
            {
                throw new ExpiredUserValidationCodeException("Validation code is expired", code: "EXPIRED_VALIDATION_CODE");
            }

            var userRow = await _userRepository.UpdateActivationCodeAsync(new ObjectId(user.Id!), activationCode);

            return userRow?.Let(it => _mapper.Map<User>(it));
        }
    }
}
