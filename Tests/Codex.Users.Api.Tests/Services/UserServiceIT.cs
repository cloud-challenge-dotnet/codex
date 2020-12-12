using Codex.Core.Exceptions;
using Codex.Users.Api.Services;
using Codex.Models.Tenants;
using Codex.Tests.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Services.Implementations;
using Codex.Models.Users;
using Codex.Core.Interfaces;
using Dapr.Client;
using System.Threading;
using Codex.Core.Models;
using Codex.Users.Api.Exceptions;

namespace Codex.Users.Api.Tests
{
    public class UserServiceIT : IClassFixture<Fixture>
    {
        public UserServiceIT()
        {
        }

        [Fact]
        public async Task FindAll() 
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();
            UserCriteria userCriteria = new();

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
                Task.FromResult(new List<User>()
                {
                    new(){ Id = "User1" },
                    new(){ Id = "User2" }
                })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var userList = await userService.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Equal(2, userList.Count);

            Assert.Equal("User1", userList[0].Id);
            Assert.Equal("User2", userList[1].Id);
        }

        [Fact]
        public async Task FindOne()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string userId = "User1";

            userRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User { Id = "User1" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var user = await userService.FindOneAsync(userId);

            Assert.NotNull(user);
            Assert.Equal(userId, user!.Id);
        }

        [Fact]
        public async Task Create()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Dictionary<string, string>>(
                new Dictionary<string, string>() { { ConfigConstant.PasswordSalt, ""} }
            ));

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "test" };

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
               Task.FromResult(new List<User>())
           );

            userRepository.Setup(x => x.InsertAsync(It.IsAny<User>())).Returns(
                Task.FromResult(new User() { Id = "User1", Login = "Login", Email = "test@gmail.com" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

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

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

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

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

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

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

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

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

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

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global", userCreator));

            Assert.Equal("USER_EMAIL_INVALID", exception.Code);
        }

        [Fact]
        public async Task Create_With_Exist_User_Login()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            userRepository.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
               Task.FromResult(new List<User>() {
                   new(){Login = "Login"}
               })
           );

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

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
               Task.FromResult(new List<User>() {
                   new(){Email = "test@gmail.com"}
               })
           );

            var userCreator = new UserCreator() { Login = "Login", Email = "test@gmail.com", Password = "test" };

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var exception = await Assert.ThrowsAsync<IllegalArgumentException>(() => userService.CreateAsync("global",userCreator));

            Assert.Equal("USER_EXISTS", exception.Code);
        }

        [Fact]
        public async Task Update()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string userId = "User1";
            var user = new User { Id = userId };

            userRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
                Task.FromResult((User?)new User { Id = userId, Login="login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var userResult = await userService.UpdateAsync(user);

            Assert.NotNull(userResult);
            Assert.Equal(userId, userResult!.Id);
            Assert.Equal("login", userResult!.Login);
        }

        [Fact]
        public async Task ActivateUser()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            string userId = "User1";
            var user = new User { Id = userId, Login = "login", ActivationCode = activationCode, ActivationValidity = DateTime.Now.AddDays(1) };

            userRepository.Setup(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var userResult = await userService.ActivateUserAsync(user, activationCode);

            Assert.NotNull(userResult);
            Assert.Equal(userId, userResult!.Id);
            Assert.Equal("login", userResult!.Login);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ActivateUser_User_ActivationCode_Null()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            string userId = "User1";
            var user = new User { Id = userId, Login = "login", ActivationCode = null, ActivationValidity = DateTime.Now.AddDays(1) };

            userRepository.Setup(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var exception = await Assert.ThrowsAsync<InvalidUserValidationCodeException>(async() => await userService.ActivateUserAsync(user, activationCode));

            Assert.NotNull(exception);
            Assert.Equal("INVALID_VALIDATION_CODE", exception.Code);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ActivateUser_Bad_ActivationCode()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            string userId = "User1";
            var user = new User { Id = userId, Login = "login", ActivationCode = "25554545445", ActivationValidity = DateTime.Now.AddDays(1) };

            userRepository.Setup(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User { Id = userId, Login = "login" })
            );

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var exception = await Assert.ThrowsAsync<InvalidUserValidationCodeException>(async () => await userService.ActivateUserAsync(user, activationCode));

            Assert.NotNull(exception);
            Assert.Equal("INVALID_VALIDATION_CODE", exception.Code);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ActivateUser_Expired_ActivationCode()
        {
            var userRepository = new Mock<IUserRepository>();
            var passwordHasher = new Mock<IPasswordHasher>();
            var daprClient = new Mock<DaprClient>();

            string activationCode = "221212313521";
            string userId = "User1";
            var user = new User { Id = userId, Login = "login", ActivationCode = activationCode, ActivationValidity = DateTime.Now.AddDays(-1) };

            var userService = new UserService(userRepository.Object, daprClient.Object, passwordHasher.Object);

            var exception = await Assert.ThrowsAsync<ExpiredUserValidationCodeException>(async () => await userService.ActivateUserAsync(user, activationCode));

            Assert.NotNull(exception);
            Assert.Equal("EXPIRED_VALIDATION_CODE", exception.Code);

            userRepository.Verify(x => x.UpdateActivationCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
