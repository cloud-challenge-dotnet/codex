using Codex.Users.Api.Controllers;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using Codex.Users.Api.Services.Interfaces;
using Codex.Models.Users;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Codex.Models.Roles;
using Microsoft.AspNetCore.Authorization;

namespace Codex.Users.Api.Tests
{
    public class UserControllerIT : IClassFixture<Fixture>
    {
        private readonly Fixture _fixture;

        public UserControllerIT(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task FindOne()
        {
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User() { Id = "Id1", Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.FindOne))?.GetCustomAttributes(typeof(AuthorizeAttribute), true);

            var result = await userController.FindOne("Id1");

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var user = Assert.IsType<User>(objectResult.Value);
            Assert.NotNull(user);
            Assert.Equal("Id1", user.Id);
            Assert.Equal("login", user.Login);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal($"{RoleConstant.TENANT_MANAGER},{RoleConstant.USER}", authorizeAttribute.Roles);
        }

        [Fact]
        public async Task FindOne_Current_User_Id()
        {
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User() { Id = "Id1", Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await userController.FindOne("Id1");

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var user = Assert.IsType<User>(objectResult.Value);
            Assert.NotNull(user);
            Assert.Equal("Id1", user.Id);
            Assert.Equal("login", user.Login);
        }


        [Fact]
        public async Task FindOne_UnAuthorize()
        {
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User() { Id = "Id1", Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id2",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await userController.FindOne("Id1");

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
            
            Assert.IsType<UnauthorizedResult>(result.Result);
        }


        [Fact]
        public async Task FindOne_NotFound()
        {
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)null)
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await userController.FindOne("Id1");

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task FindAll()
        {
            UserCriteria userCriteria = new();
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
                Task.FromResult(new List<User>(){
                    new() { Id = "Id1" },
                    new() { Id = "Id2" }
                })
            );

            var userController = new UserController(
                userService.Object
            );

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.FindAll))?.GetCustomAttributes(typeof(AuthorizeAttribute), true);

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );

            var result = await userController.FindAll(userCriteria);

            userService.Verify(x => x.FindAllAsync(It.IsAny<UserCriteria>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var userList = Assert.IsType<List<User>>(objectResult.Value);
            Assert.NotNull(userList);
            Assert.Equal(2, userList!.Count);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task CreateUser()
        {
            UserCreator userCreator = new() { Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.CreateAsync(It.IsAny<UserCreator>())).Returns(
                Task.FromResult(new User() { Id = "Id1", Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.CreateUser))?.GetCustomAttributes(typeof(AuthorizeAttribute), true);

            var result = await userController.CreateUser(userCreator);

            userService.Verify(x => x.CreateAsync(It.IsAny<UserCreator>()), Times.Once);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(userController.FindOne), createdAtActionResult.ActionName);
            var user = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.NotNull(user);
            Assert.Equal("Id1", user.Id);
            Assert.Equal("login", user.Login);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task UpdateUser()
        {
            string currentUserId = "Id1";
            User user = new() { Id = "Id1", Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
                Task.FromResult((User?)new User() { Id = "Id1", Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.UpdateUser))?.GetCustomAttributes(typeof(AuthorizeAttribute), true);

            var httpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: currentUserId,
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser("Id1", user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(userController.FindOne), acceptedAtActionResult.ActionName);
            var userResult = Assert.IsType<User>(acceptedAtActionResult.Value);
            Assert.NotNull(user);
            Assert.Equal("Id1", user.Id);
            Assert.Equal("login", user.Login);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<AuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal($"{RoleConstant.TENANT_MANAGER},{RoleConstant.USER}", authorizeAttribute.Roles);
        }


        [Fact]
        public async Task UpdateUser_Not_Found_User()
        {
            string currentUserId = "Id1";
            User user = new() { Id = "Id1", Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
                Task.FromResult((User?)null)
            );

            var userController = new UserController(
                userService.Object
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentUserId),
                        new Claim(ClaimTypes.Role, RoleConstant.TENANT_MANAGER)
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser("Id1", user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task UpdateCurrentUser()
        {
            string userId = "Id1";
            User user = new() { Id="Id1", Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User() { Id = "Id1", Login = "login" })
            );

            userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
                Task.FromResult((User?)new User() { Id = "Id1", Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(userId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(userController.FindOne), acceptedAtActionResult.ActionName);
            var userResult = Assert.IsType<User>(acceptedAtActionResult.Value);
            Assert.NotNull(user);
            Assert.Equal("Id1", user.Id);
            Assert.Equal("login", user.Login);
        }

        [Fact]
        public async Task UpdateCurrentUser_With_Not_Found_User()
        {
            string userId = "Id1";
            User user = new() { Id = "Id1", Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)null)
            );

            var userController = new UserController(
                userService.Object
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(userId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Id1", notFoundObjectResult.Value);
        }

        [Fact]
        public async Task UpdateUser_UnAuthorized()
        {
            string currentUserId = "Id2";
            string userId = "Id1";
            User user = new() { Id = userId, Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((User?)null)
            );

            var userController = new UserController(
                userService.Object
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentUserId)
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(userId, user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);

            Assert.IsType<UnauthorizedResult>(result.Result);
        }
    }
}
