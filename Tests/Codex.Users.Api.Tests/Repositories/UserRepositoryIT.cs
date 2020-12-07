using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id);
            Assert.Equal("5fb92118da7ed3521e4a7d60", userList[1].Id);
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
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id);
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
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id);
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
            Assert.Equal("5fb92118da7ed3521e4a7d59", userList[0].Id);
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
        public async Task Update()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/users.json");

            var user = await _userRepository.UpdateAsync(new() {
                Id = "5fb92118da7ed3521e4a7d59",
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
                EmailConfirmed = true,
                PhoneConfirmed = true,
                PasswordHash = "test",
                Active = true
            });

            Assert.NotNull(user);
            Assert.Equal("user-login", user!.Login);
            Assert.Equal("user-email", user!.Email);
            Assert.Equal("user-firstName", user!.FirstName);
            Assert.Equal("user-lastName", user!.LastName);
            Assert.Equal("user-phoneNumber", user!.PhoneNumber);
            Assert.NotNull(user!.Roles);
            Assert.Equal(2, user!.Roles.Count);
            Assert.Equal("ADMIN", user!.Roles[0]);
            Assert.Equal("USER", user!.Roles[1]);
            Assert.True(user!.Active);

            //Not updated
            Assert.Equal("5fb92118da7ed3521e4a7d59", user!.Id);
            Assert.Equal("G3JsGuJkkF/nLP27fnsyzGpL+jIU0YgVIVd5qoKiepE=", user!.PasswordHash);
            Assert.False(user!.EmailConfirmed);
            Assert.False(user!.PhoneConfirmed);
        }
    }
}
