using Codex.Users.Api.Controllers;
using Codex.Tests.Framework;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Codex.Users.Api.Services.Interfaces;
using Codex.Models.Users;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Codex.Models.Roles;
using Codex.Core.Security;
using MongoDB.Bson;

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
            var userId = ObjectId.GenerateNewId();

            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
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

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.FindOne))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await userController.FindOne(userId.ToString());

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var user = Assert.IsType<User>(objectResult.Value);
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("login", user.Login);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal($"{RoleConstant.TENANT_MANAGER},{RoleConstant.USER}", authorizeAttribute.Roles);
        }

        [Fact]
        public async Task FindOne_Current_User_Id()
        {
            var userId = ObjectId.GenerateNewId();

            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: userId.ToString(),
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await userController.FindOne(userId.ToString());

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);

            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var user = Assert.IsType<User>(objectResult.Value);
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("login", user.Login);
        }


        [Fact]
        public async Task FindOne_UnAuthorize()
        {
            var userId = ObjectId.GenerateNewId();

            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
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

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Never);
            
            Assert.IsType<UnauthorizedResult>(result.Result);
        }


        [Fact]
        public async Task FindOne_NotFound()
        {
            var userId = ObjectId.GenerateNewId();

            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)null)
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: userId.ToString(),
                userName: "login",
                roles: new() { RoleConstant.USER }
            );

            var result = await userController.FindOne(userId.ToString());

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(userId.ToString(), notFoundObjectResult.Value);
        }

        [Fact]
        public async Task FindAll()
        {
            var userId1 = ObjectId.GenerateNewId();
            var userId2 = ObjectId.GenerateNewId();

            UserCriteria userCriteria = new();
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
                Task.FromResult(new List<User>(){
                    new() { Id = userId1 },
                    new() { Id = userId2 }
                })
            );

            var userController = new UserController(
                userService.Object
            );

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.FindAll))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            userController.ControllerContext.HttpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: userId1.ToString(),
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
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task CreateUser()
        {
            var userId = ObjectId.GenerateNewId();
            UserCreator userCreator = new() { Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<UserCreator>())).Returns(
                Task.FromResult(new User() { Id = userId, Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            userController.ControllerContext.HttpContext = new DefaultHttpContext();

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.CreateUser))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var result = await userController.CreateUser(userCreator);

            userService.Verify(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<UserCreator>()), Times.Once);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(userController.FindOne), createdAtActionResult.ActionName);
            var user = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("login", user.Login);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal(RoleConstant.TENANT_MANAGER, authorizeAttribute.Roles);
        }

        [Fact]
        public async Task UpdateUser()
        {
            var currentUserId = ObjectId.GenerateNewId();
            User user = new() { Id = currentUserId, Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
                Task.FromResult((User?)new User() { Id = currentUserId, Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            var authorizeAttributes = userController.GetType().GetMethod(nameof(UserController.UpdateUser))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

            var httpContext = Fixture.CreateHttpContext(
                tenantId: "global",
                userId: currentUserId.ToString(),
                userName: "login",
                roles: new() { RoleConstant.TENANT_MANAGER }
            );

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(currentUserId.ToString(), user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Never);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(userController.FindOne), acceptedAtActionResult.ActionName);
            var userResult = Assert.IsType<User>(acceptedAtActionResult.Value);
            Assert.NotNull(user);
            Assert.Equal(currentUserId, user.Id);
            Assert.Equal("login", user.Login);

            Assert.NotNull(authorizeAttributes);
            Assert.Single(authorizeAttributes);
            var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
            Assert.Equal($"{RoleConstant.TENANT_MANAGER},{RoleConstant.USER}", authorizeAttribute.Roles);
        }


        [Fact]
        public async Task UpdateUser_Not_Found_User()
        {
            var currentUserId = ObjectId.GenerateNewId();
            User user = new() { Id = currentUserId, Login = "login" };
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
                        new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()),
                        new Claim(ClaimTypes.Role, RoleConstant.TENANT_MANAGER)
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(currentUserId.ToString(), user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Never);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(currentUserId.ToString(), notFoundObjectResult.Value);
        }

        [Fact]
        public async Task UpdateCurrentUser()
        {
            var userId = ObjectId.GenerateNewId();
            User user = new() { Id = userId, Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
            );

            userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(userId.ToString(), user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

            var acceptedAtActionResult = Assert.IsType<AcceptedAtActionResult>(result.Result);
            Assert.Equal(nameof(userController.FindOne), acceptedAtActionResult.ActionName);
            var userResult = Assert.IsType<User>(acceptedAtActionResult.Value);
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("login", user.Login);
        }

        [Fact]
        public async Task UpdateCurrentUser_With_Not_Found_User()
        {
            var userId = ObjectId.GenerateNewId();
            User user = new() { Id = userId, Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
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
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(userId.ToString(), user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);

            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(userId.ToString(), notFoundObjectResult.Value);
        }

        [Fact]
        public async Task UpdateUser_UnAuthorized()
        {
            var currentUserId = ObjectId.GenerateNewId();
            var userId = ObjectId.GenerateNewId();
            User user = new() { Id = userId, Login = "login" };
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
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
                        new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString())
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.UpdateUser(userId.ToString(), user);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Never);
            userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);

            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task ActivateUser()
        {
            string activationCode = "1121313534";
            var userId = ObjectId.GenerateNewId();
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
            );

            userService.Setup(x => x.ActivateUserAsync(It.IsAny<User>(), It.IsAny<string>())).Returns(
                Task.FromResult((User?)new User() { Id = userId, Login = "login" })
            );

            var userController = new UserController(
                userService.Object
            );

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.ActivateUser(userId.ToString(), activationCode);
            var objectResult = Assert.IsType<OkObjectResult>(result.Result);
            var user = Assert.IsType<User>(objectResult.Value);
            Assert.NotNull(user);
            Assert.Equal(userId, user.Id);
            Assert.Equal("login", user.Login);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);
            userService.Verify(x => x.ActivateUserAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ActivateUser_Not_Found_User()
        {
            string activationCode = "1121313534";
            var userId = ObjectId.GenerateNewId();
            var userService = new Mock<IUserService>();

            userService.Setup(x => x.FindOneAsync(It.IsAny<ObjectId>())).Returns(
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
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuthType")
                )
            };

            userController.ControllerContext.HttpContext = httpContext;

            var result = await userController.ActivateUser(userId.ToString(), activationCode);
            var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(userId.ToString(), notFoundObjectResult.Value);

            userService.Verify(x => x.FindOneAsync(It.IsAny<ObjectId>()), Times.Once);
            userService.Verify(x => x.ActivateUserAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }
    }
}
