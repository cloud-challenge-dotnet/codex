using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Security;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Roles;
using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.GrpcServices;
using Codex.Users.Api.MappingProfiles;
using Codex.Users.Api.Services.Interfaces;
using CodexGrpc.Users;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace Codex.Users.Api.Tests.GrpcServices;

public class UserGrpcServiceIt : IClassFixture<Fixture>
{
    private readonly IMapper _mapper;

    public UserGrpcServiceIt()
    {
        //auto mapper configuration
        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile<CoreMappingProfile>();
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<Codex.Core.MappingProfiles.GrpcMappingProfile>();
            cfg.AddProfile<GrpcMappingProfile>();
        });
        _mapper = mockMapper.CreateMapper();
    }
    
    [Fact]
    public async Task FindOne()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id1",
                userName: "login",
                roles: new() { RoleConstant.TenantManager }
            )
        );

        var authorizeAttributes = userGrpcService.GetType().GetMethod(nameof(UserGrpcService.FindOne))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var user = await userGrpcService.FindOne(
            new(){Id = userId},
            serverCallContext
        );

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("login", user.Login);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal($"{RoleConstant.TenantManager},{RoleConstant.User}", authorizeAttribute.Roles);
    }

    [Fact]
    public async Task FindOne_Current_User_Id()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: userId,
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var user = await userGrpcService.FindOne(
            new(){Id = userId},
            serverCallContext
        );

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("login", user.Login);
    }


    [Fact]
    public async Task FindOne_UnAuthorize()
    {
        var userId = ObjectId.GenerateNewId().ToString();

        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: "Id2",
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.FindOne(
            new(){Id = "Id1"},
            serverCallContext
        ));

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
        
        Assert.Equal(StatusCode.Unauthenticated, rpcException.Status.StatusCode);
        Assert.Equal("Id1", rpcException.Status.Detail);
    }


    [Fact]
    public async Task FindOne_NotFound()
    {
        var userId = ObjectId.GenerateNewId();

        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)null)
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: userId.ToString(),
                userName: "login",
                roles: new() { RoleConstant.User }
            )
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.FindOne(
            new(){Id = userId.ToString()},
            serverCallContext
        ));

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal(userId.ToString(), rpcException.Status.Detail);
    }

    [Fact]
    public async Task FindAll()
    {
        var userId1 = ObjectId.GenerateNewId().ToString();
        var userId2 = ObjectId.GenerateNewId().ToString();

        FindAllUserRequest findAllUserRequest = new()
        {
            Criteria = new()
        };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>())).Returns(
            Task.FromResult(new List<User>(){
                new() { Id = userId1 },
                new() { Id = userId2 }
            })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var authorizeAttributes = userGrpcService.GetType().GetMethod(nameof(UserGrpcService.FindAll))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: userId1,
                userName: "login",
                roles: new() { RoleConstant.TenantManager }
            )
        );

        var userListResponse = await userGrpcService.FindAll(findAllUserRequest, serverCallContext);

        userService.Verify(x => x.FindAllAsync(It.IsAny<UserCriteria>()), Times.Once);

        var userList = _mapper.Map<List<User>>(userListResponse.Users);
        Assert.NotNull(userList);
        Assert.Equal(2, userList.Count);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task CreateUser()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        UserModel userCreator = new() { Login = "login" };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<UserCreator>())).Returns(
            Task.FromResult(new User() { Id = userId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        var serverCallContext = Fixture.CreateServerCallContext(new DefaultHttpContext());

        var authorizeAttributes = userGrpcService.GetType().GetMethod(nameof(UserGrpcService.Create))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var user = await userGrpcService.Create(userCreator, serverCallContext);

        userService.Verify(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<UserCreator>()), Times.Once);

        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("login", user.Login);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal(RoleConstant.TenantManager, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task UpdateUser()
    {
        var currentUserId = ObjectId.GenerateNewId().ToString();
        UserModel user = new() { Id = currentUserId, Login = "login" };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
            Task.FromResult((User?)new User() { Id = currentUserId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var authorizeAttributes = userGrpcService.GetType().GetMethod(nameof(UserGrpcService.Update))?.GetCustomAttributes(typeof(TenantAuthorizeAttribute), true);

        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(
                tenantId: "global",
                userId: currentUserId,
                userName: "login",
                roles: new() { RoleConstant.TenantManager }
            )
        );

        var userResult = await userGrpcService.Update(user, serverCallContext);

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
        userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

        Assert.NotNull(userResult);
        Assert.Equal(currentUserId, userResult.Id);
        Assert.Equal("login", userResult.Login);

        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        var authorizeAttribute = Assert.IsType<TenantAuthorizeAttribute>(authorizeAttributes![0]);
        Assert.Equal($"{RoleConstant.TenantManager},{RoleConstant.User}", authorizeAttribute.Roles);
    }


    [Fact]
    public async Task UpdateUser_Not_Found_User()
    {
        var currentUserId = ObjectId.GenerateNewId().ToString();
        UserModel user = new() { Id = currentUserId, Login = "login" };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
            Task.FromResult((User?)null)
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentUserId),
                        new Claim(ClaimTypes.Role, RoleConstant.TenantManager)
                    }, "TestAuthType")
                )
            }
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.Update(user, serverCallContext));

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
        userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal(user.Id, rpcException.Status.Detail);
    }

    [Fact]
    public async Task UpdateCurrentUser()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        UserModel user = new() { Id = userId, Login = "login" };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        userService.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            }
        );

        var userResult = await userGrpcService.Update(user, serverCallContext);

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
        userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);

        Assert.NotNull(userResult);
        Assert.Equal(userId, userResult.Id);
        Assert.Equal("login", userResult.Login);
    }

    [Fact]
    public async Task UpdateCurrentUser_With_Not_Found_User()
    {
        var userId = ObjectId.GenerateNewId().ToString();
        UserModel user = new() { Id = userId, Login = "login" };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)null)
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            }
        );
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.Update(user, serverCallContext));

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
        userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        
        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal(userId, rpcException.Status.Detail);
    }

    [Fact]
    public async Task UpdateUser_UnAuthorized()
    {
        var currentUserId = ObjectId.GenerateNewId().ToString();
        var userId = ObjectId.GenerateNewId().ToString();
        UserModel user = new() { Id = userId, Login = "login" };
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)null)
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentUserId)
                    }, "TestAuthType")
                )
            }
        );
        
        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.Update(user, serverCallContext));

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Never);
        userService.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);

        Assert.Equal(StatusCode.Unauthenticated, rpcException.Status.StatusCode);
        Assert.Equal(userId, rpcException.Status.Detail);
    }

    [Fact]
    public async Task UpdatePassword()
    {
        var password = "myPassword";
        var userId = ObjectId.GenerateNewId().ToString();
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login", PasswordHash = "5315645644" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            }
        );

        var userResult = await userGrpcService.UpdatePassword(
            new()
            {
                UserId = userId,
                Password = password
            },
            serverCallContext
        );

        userService.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.NotNull(userResult);
        Assert.Equal(userId, userResult.Id);
        Assert.Equal("login", userResult.Login);
        Assert.Null(userResult.PasswordHash);
    }

    [Fact]
    public async Task UpdatePassword_With_Not_Found_User()
    {
        var password = "myPassword";
        var userId = ObjectId.GenerateNewId().ToString();
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
            Task.FromResult((User?)null)
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            }
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.UpdatePassword(
            new()
            {
                UserId = userId,
                Password = password
            },
            serverCallContext
        ));

        userService.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        
        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal(userId, rpcException.Status.Detail);
    }

    [Fact]
    public async Task UpdatePassword_UnAuthorized()
    {
        var password = "myPassword";
        var currentUserId = ObjectId.GenerateNewId().ToString();
        var userId = ObjectId.GenerateNewId().ToString();
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login", PasswordHash = "5315645644" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );
        
        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, currentUserId)
                    }, "TestAuthType")
                )
            }
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.UpdatePassword(
            new()
            {
                UserId = userId,
                Password = password
            },
            serverCallContext
        ));

        userService.Verify(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        Assert.Equal(StatusCode.Unauthenticated, rpcException.Status.StatusCode);
        Assert.Equal(userId, rpcException.Status.Detail);
    }

    [Fact]
    public async Task ActivateUser()
    {
        string activationCode = "1121313534";
        var userId = ObjectId.GenerateNewId().ToString();
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        userService.Setup(x => x.ActivateUserAsync(It.IsAny<User>(), It.IsAny<string>())).Returns(
            Task.FromResult((User?)new User() { Id = userId, Login = "login" })
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    }, "TestAuthType")
                )
            }
        );

        var user = await userGrpcService.ActivateUser(
            new(){
                UserId = userId,
                ActivationCode = activationCode
            },
            serverCallContext
        );
        
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("login", user.Login);

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
        userService.Verify(x => x.ActivateUserAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ActivateUser_Not_Found_User()
    {
        string activationCode = "1121313534";
        var userId = ObjectId.GenerateNewId();
        var userService = new Mock<IUserService>();

        userService.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
            Task.FromResult((User?)null)
        );

        var userGrpcService = new UserGrpcService(
            _mapper,
            userService.Object
        );

        var serverCallContext = Fixture.CreateServerCallContext(
            new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "TestAuthType")
                )
            }
        );

        var rpcException = await Assert.ThrowsAsync<RpcException>(async ()=> await userGrpcService.ActivateUser(
            new(){
                UserId = userId.ToString(),
                ActivationCode = activationCode
            },
            serverCallContext
        ));
        
        Assert.Equal(StatusCode.NotFound, rpcException.Status.StatusCode);
        Assert.Equal(userId.ToString(), rpcException.Status.Detail);

        userService.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
        userService.Verify(x => x.ActivateUserAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
}