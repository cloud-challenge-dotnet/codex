using AutoMapper;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Exceptions;
using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Exceptions;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Repositories.Models;
using Codex.Users.Api.Resources;
using Codex.Users.Api.Services.Implementations;
using Dapr.Client;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Users.Api.Tests
{
    public class UserServiceIT : IClassFixture<Fixture>
    {
        private readonly IMapper _mapper;

        private readonly IStringLocalizer<UserResource> _stringLocalizer;

        public UserServiceIT()
        {
            //auto mapper configuration
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile(new CoreMappingProfile());
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mockMapper.CreateMapper();

            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            _stringLocalizer = new StringLocalizer<UserResource>(factory);
        }

        [Fact]
        public async Task FindAll()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();
            UserCriteria userCriteria = new();
            var userId1 = ObjectId.GenerateNewId();
            var userId2 = ObjectId.GenerateNewId();

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
                Task.FromResult(new List<UserRow>()
                {
                    new(){ Id = userId1 },
                    new(){ Id = userId2 }
                })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var userList = await userService.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Equal(2, userList.Count);

            Assert.Equal(userId1.ToString(), userList[0].Id);
            Assert.Equal(userId2.ToString(), userList[1].Id);
        }

        [Fact]
        public async Task FindOne()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();
            var userId = ObjectId.GenerateNewId();

            userRepository.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((UserRow?)new UserRow { Id = userId })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var user = await userService.FindOneAsync(userId.ToString());

            Assert.NotNull(user);
            Assert.Equal(userId.ToString(), user!.Id);
        }

        [Fact]
        public async Task Create()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();
            var userId = ObjectId.GenerateNewId();

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string>() { { ConfigConstant.PasswordSalt, "" } }
            ));

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "test" };

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
               Task.FromResult(new List<UserRow>())
           );

            userRepository.Setup(x => x.InsertAsync(It.IsAny<UserRow>())).Returns(
                Task.FromResult(new UserRow() { Id = userId, Login = "Login", Email = "test@gmail.com" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var user = await userService.CreateAsync("global", userCreator);

            Assert.NotNull(user);
            Assert.Equal(userCreator.Login, user.Login);
            Assert.Equal(userCreator.Email, user.Email);
        }

        [Fact]
        public async Task Create_With_Null_Password()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = null };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_PASSWORD_INVALID", exception.Code);
        }

        [Fact]
        public async Task Create_With_Empty_Password()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "" };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_PASSWORD_INVALID", exception.Code);
        }

        [Fact]
        public async Task Create_With_Empty_Login()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            var userCreator = new UserCreator() { Login = "", Email = "test@gmail.com", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_LOGIN_INVALID", exception.Code);
        }

        [Fact]
        public async Task Create_With_Empty_Email()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            var userCreator = new UserCreator() { Login = "Login", Email = "", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_EMAIL_INVALID", exception.Code);
        }

        [Fact]
        public async Task Create_With_Invalid_Email_Format()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            var userCreator = new UserCreator() { Login = "Login", Email = "test.com", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_EMAIL_INVALID", exception.Code);
        }

        [Fact]
        public async Task Create_With_Exist_User_Login()
        {
            //var stringLocalizer = new Mock<IStringLocalizer<UserResource>>();
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
               Task.FromResult(new List<UserRow>() {
                   new(){Login = "Login"}
               })
           );

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_EXISTS", exception.Code);
        }

        [Fact]
        public async Task Create_With_Exist_User_Email()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
               Task.FromResult(new List<UserRow>() {
                   new(){Email = "test@gmail.com"}
               })
           );

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_EXISTS", exception.Code);
        }

        [Fact]
        public async Task Update()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            var userId = ObjectId.GenerateNewId();
            var user = new User { Id = userId.ToString() };

            userRepository.Setup(x => x.UpdateAsync(It.IsAny<UserRow>())).Returns(
                Task.FromResult((UserRow?)new UserRow { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var userResult = await userService.UpdateAsync(user);

            Assert.NotNull(userResult);
            Assert.Equal(userId.ToString(), userResult!.Id);
            Assert.Equal("login", userResult!.Login);
        }

        [Fact]
        public async Task ActivateUser()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            var userId = ObjectId.GenerateNewId();
            var user = new User { Id = userId.ToString(), Login = "login", ActivationCode = activationCode, ActivationValidity = DateTime.Now.AddDays(1) };

            userRepository.Setup(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>())).Returns(
                Task.FromResult((UserRow?)new UserRow { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var userResult = await userService.ActivateUserAsync(user, activationCode);

            Assert.NotNull(userResult);
            Assert.Equal(userId.ToString(), userResult!.Id);
            Assert.Equal("login", userResult!.Login);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ActivateUser_User_ActivationCode_Null()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            var userId = ObjectId.GenerateNewId();
            var user = new User { Id = userId.ToString(), Login = "login", ActivationCode = null, ActivationValidity = DateTime.Now.AddDays(1) };

            userRepository.Setup(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>())).Returns(
                Task.FromResult((UserRow?)new UserRow { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<InvalidUserValidationCodeException>(async () => await userService.ActivateUserAsync(user, activationCode));

            Assert.NotNull(exception);
            Assert.Equal("INVALID_VALIDATION_CODE", exception.Code);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ActivateUser_Bad_ActivationCode()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            var userId = ObjectId.GenerateNewId();
            var user = new User { Id = userId.ToString(), Login = "login", ActivationCode = "25554545445", ActivationValidity = DateTime.Now.AddDays(1) };

            userRepository.Setup(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>())).Returns(
                Task.FromResult((UserRow?)new UserRow { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<InvalidUserValidationCodeException>(async () => await userService.ActivateUserAsync(user, activationCode));

            Assert.NotNull(exception);
            Assert.Equal("INVALID_VALIDATION_CODE", exception.Code);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ActivateUser_Expired_ActivationCode()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            var userId = ObjectId.GenerateNewId();
            var user = new User { Id = userId.ToString(), Login = "login", ActivationCode = activationCode, ActivationValidity = DateTime.Now.AddDays(-1) };

            var userService = new UserService(userRepository.Object, daprClient.Object,
                passwordHasher.Object, _mapper, _stringLocalizer);

            var exception = await Assert.ThrowsAsync<ExpiredUserValidationCodeException>(async () => await userService.ActivateUserAsync(user, activationCode));

            Assert.NotNull(exception);
            Assert.Equal("EXPIRED_VALIDATION_CODE", exception.Code);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<ObjectId>(), It.IsAny<string>()), Times.Never);
        }
    }
}
