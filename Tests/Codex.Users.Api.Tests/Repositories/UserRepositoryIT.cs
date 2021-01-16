using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Users.Api.Tests
{
    public class UserRepositoryIT : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;
        private readonly IUserRepository _userRepository;

        public UserRepositoryIT(DbFixture fixture)
        {
            _fixture = fixture;
            _userRepository = _fixture.Services.GetService<IUserRepository>()!;
        }

        [Fact]
        public async Task FindAll()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            UserCriteria userCriteria = new();
            var userList = await _userRepository.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Equal(2, userList.Count);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id.ToString());
            Assert.Equal("5fb92118da7ed3521e4a7d60", userList[1].Id.ToString());
        }


        [Fact]
        public async Task FindAll_By_Login()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            UserCriteria userCriteria = new(Login: "user1");
            var userList = await _userRepository.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Single(userList);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id.ToString());
            Assert.Equal("user1", userList[0].Login);
            Assert.Equal("test1@gmail.com", userList[0].Email);
        }

        [Fact]
        public async Task FindAll_By_Email()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            UserCriteria userCriteria = new(Email: "test1@gmail.com");
            var userList = await _userRepository.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Single(userList);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id.ToString());
            Assert.Equal("user1", userList[0].Login);
            Assert.Equal("test1@gmail.com", userList[0].Email);
        }

        [Fact]
        public async Task FindAll_By_Login_And_Email()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            UserCriteria userCriteria = new(Login: "user1", Email: "test1@gmail.com");
            var userList = await _userRepository.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Single(userList);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id.ToString());
            Assert.Equal("user1", userList[0].Login);
            Assert.Equal("test1@gmail.com", userList[0].Email);
        }

        [Fact]
        public async Task FindAll_By_Bad_Login_And_Email()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            UserCriteria userCriteria = new(Login: "user1", Email: "test2@gmail.com");
            var userList = await _userRepository.FindAllAsync(userCriteria);

            Assert.NotNull(userList);
            Assert.Empty(userList);
        }

        [Fact]
        public async Task Insert()
        {
            await _fixture.DropDatabaseAsync();

            var user = await _userRepository.InsertAsync(new()
            {
                Id = new ObjectId("5fb92118da7ed3521e4a7d59"),
                Login = "user-login",
                Email = "user-email",
                FirstName = "user-firstName",
                LastName = "user-lastName",
                PhoneNumber = "user-phoneNumber",
                Roles = new()
                {
                    "ADMIN",
                    "USER"
                },
                ActivationCode = "123456",
                ActivationValidity = DateTime.Now.AddDays(1),
                PasswordHash = "test",
                Active = true
            });

            Assert.NotNull(user);
            Assert.Equal("user-login", user!.Login);
            Assert.True(user!.CreationDate > DateTime.Now.Date);
            Assert.True(user!.ModificationDate > DateTime.Now.Date);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", user!.Id.ToString());
        }

        [Fact]
        public async Task Update()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            var user = await _userRepository.UpdateAsync(new()
            {
                Id = new ObjectId("5fb92118da7ed3521e4a7d59"),
                Login = "user-login",
                Email = "user-email",
                FirstName = "user-firstName",
                LastName = "user-lastName",
                PhoneNumber = "user-phoneNumber",
                Roles = new()
                {
                    "ADMIN",
                    "USER"
                },
                LanguageCultureName = "en-GB",
                ActivationCode = "123456",
                ActivationValidity = DateTime.Now.AddDays(1),
                PasswordHash = "test",
                Active = true
            });

            Assert.NotNull(user);
            Assert.Equal("user-login", user!.Login);
            Assert.Equal("user-email", user!.Email);
            Assert.Equal("user-firstName", user!.FirstName);
            Assert.Equal("user-lastName", user!.LastName);
            Assert.Equal("user-phoneNumber", user!.PhoneNumber);
            Assert.NotEqual(new DateTime(2020, 12, 12), user!.ModificationDate);
            Assert.NotNull(user!.Roles);
            Assert.Equal(2, user!.Roles.Count);
            Assert.Equal("ADMIN", user!.Roles[0]);
            Assert.Equal("USER", user!.Roles[1]);
            Assert.Equal("en-GB", user!.LanguageCultureName);
            Assert.Equal("123456", user!.ActivationCode);
            Assert.Equal("test", user!.PasswordHash);
            Assert.True(user!.Active);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", user!.Id.ToString());
            Assert.Equal(new DateTime(2020, 12, 12), user!.CreationDate);
        }


        [Fact]
        public async Task UpdatePassword()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            var user = await _userRepository.UpdatePasswordAsync(new ObjectId("5fb92118da7ed3521e4a7d59"), "test");

            Assert.NotNull(user);
            Assert.NotEqual(new DateTime(2020, 12, 12), user!.ModificationDate);
            Assert.Equal("test", user!.PasswordHash);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", user!.Id.ToString());
        }


        [Fact]
        public async Task UpdateActivationCode()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            var user = await _userRepository.UpdateActivationCodeAsync(new ObjectId("5fb92118da7ed3521e4a7d59"), "56458646456");

            Assert.NotNull(user);
            Assert.NotEqual(new DateTime(2020, 12, 12), user!.ModificationDate);
            Assert.Equal("56458646456", user!.ActivationCode);
            Assert.NotNull(user!.ActivationValidity);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", user!.Id.ToString());
            Assert.Equal(new DateTime(2020, 12, 12), user!.CreationDate);
        }
    }
}
