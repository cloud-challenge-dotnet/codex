using Codex.Core.Cache;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Models.Roles;
using Codex.Models.Tenants;
using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Exceptions;
using Codex.Users.Api.Resources;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Tools.AutoMapper;
using Codex.Users.Api.MappingProfiles;
using CodexGrpc.Users;
using Grpc.Core;
using Grpc.Core.Testing;
using Xunit;
using AuthenticationService = Codex.Users.Api.Services.Implementations.AuthenticationService;

namespace Codex.Users.Api.Tests.Services;

public class AuthenticationServiceIt : IClassFixture<Fixture>
{
    private readonly IStringLocalizer<UserResource> _stringLocalizer;
    
    private readonly IMapper _mapper;

    public AuthenticationServiceIt()
    {
        var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        _stringLocalizer = new StringLocalizer<UserResource>(factory);
        
        //auto mapper configuration
        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile(new CoreMappingProfile());
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<Codex.Core.MappingProfiles.GrpcMappingProfile>();
            cfg.AddProfile<GrpcMappingProfile>();
        });
        _mapper = mockMapper.CreateMapper();
    }

    [Fact]
    public async Task AuthenticateAsync()
    {
        var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");
        var userId = ObjectId.GenerateNewId().ToString();

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var configurationSection = new Mock<IConfigurationSection>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        tenantCacheService.Setup(x => x.GetTenantAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new Tenant("global")));

        userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
            .Returns(Task.FromResult(new List<User>(){
                    new () { Id= userId, Login = "Login", PasswordHash="123123313" , Active = true, Roles = new(){ RoleConstant.Admin } }
                })
            );

        roleService.Setup(x => x.GetRoles())
            .Returns(new List<Role>(){
                new (RoleConstant.Admin, UpperRoleCode: null),
                new (RoleConstant.User, UpperRoleCode: RoleConstant.Admin)
            });

        daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() { { ConfigConstant.PasswordSalt, "" } }
            ));

        passwordHasher.Setup(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("123123313");

        //jwtToken
        configurationSection.Setup(c => c.Value).Returns("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e");

        configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);

        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        var auth = await authenticationService.AuthenticateAsync(userLogin);

        tenantCacheService.Verify(x => x.GetTenantAsync(It.IsAny<string>()), Times.Once);
        userService.Verify(x => x.FindAllAsync(It.IsAny<UserCriteria>()), Times.Once);
        roleService.Verify(x => x.GetRoles(), Times.Once);
        daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
        daprClient.Verify(x => x.InvokeMethodAsync<List<User>>(
            It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        passwordHasher.Verify(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.NotNull(auth);
        Assert.Equal(userId, auth.Id);
        Assert.NotNull(auth.Token);
    }

    [Fact]
    public async Task AuthenticateAsync_Empty_Login()
    {
        var userLogin = new UserLogin(Login: "", Password: "test", TenantId: "global");

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () => await authenticationService.AuthenticateAsync(userLogin)
        );

        Assert.Equal("INVALID_LOGIN", exception.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_Empty_Password()
    {
        var userLogin = new UserLogin(Login: "Login", Password: "", TenantId: "global");

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () => await authenticationService.AuthenticateAsync(userLogin)
        );

        Assert.Equal("INVALID_LOGIN", exception.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_User_Not_Found()
    {
        var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        tenantCacheService.Setup(x => x.GetCacheAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<Tenant?>(new Tenant("global")));

        userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
            .Returns(Task.FromResult(new List<User>())
            );

        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () => await authenticationService.AuthenticateAsync(userLogin)
        );

        Assert.Equal("INVALID_LOGIN", exception.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_User_Invalid_Password()
    {
        var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");
        var userId = ObjectId.GenerateNewId().ToString();

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var configurationSection = new Mock<IConfigurationSection>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        tenantCacheService.Setup(x => x.GetCacheAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<Tenant?>(new Tenant("global")));

        userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
            .Returns(Task.FromResult(new List<User>(){
                    new () { Id= userId, PasswordHash="123123313" , Active = true, Roles = new(){ RoleConstant.Admin } }
                })
            );

        roleService.Setup(x => x.GetRoles())
            .Returns(new List<Role>(){
                new (RoleConstant.Admin, UpperRoleCode: null),
                new (RoleConstant.User, UpperRoleCode: RoleConstant.Admin)
            });

        daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() { { ConfigConstant.PasswordSalt, "" } }
            ));

        passwordHasher.Setup(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("447899898");

        //jwtToken
        configurationSection.Setup(c => c.Value).Returns("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e");

        configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);


        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
            async () => await authenticationService.AuthenticateAsync(userLogin)
        );

        Assert.Equal("INVALID_LOGIN", exception.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_User_Disabled()
    {
        var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "global");
        var userId = ObjectId.GenerateNewId().ToString();

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();

        tenantCacheService.Setup(x => x.GetCacheAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<Tenant?>(new Tenant("global")));

        userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
            .Returns(Task.FromResult(new List<User>() {
                    new () { Id= userId, Login= "Login", PasswordHash="123123313" , Active = false, Roles = new(){ RoleConstant.Admin } }
                })
            );

        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        var exception = await Assert.ThrowsAsync<DisabledUserException>(
            async () => await authenticationService.AuthenticateAsync(userLogin)
        );

        Assert.Equal("DISABLED_USER", exception.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_With_Global_Tenant_User()
    {
        var userLogin = new UserLogin(Login: "Login", Password: "test", TenantId: "demo");
        var userId = ObjectId.GenerateNewId().ToString();

        var logger = new Mock<ILogger<AuthenticationService>>();
        var daprClient = new Mock<DaprClient>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var userService = new Mock<IUserService>();
        var configuration = new Mock<IConfiguration>();
        var configurationSection = new Mock<IConfigurationSection>();
        var roleService = new Mock<IRoleService>();
        var tenantCacheService = new Mock<ITenantCacheService>();
        var userServiceClient = new Mock<UserService.UserServiceClient>();

        tenantCacheService.Setup(x => x.GetTenantAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new Tenant("demo")));

        userService.Setup(x => x.FindAllAsync(It.IsAny<UserCriteria>()))
            .Returns(Task.FromResult(new List<User>()));
        
        var fakeCall = TestCalls.AsyncUnaryCall(Task.FromResult(new UserListResponse()
            {
                Users = {
                    new UserModel()
                    {
                        Id = userId, Login = "Login", PasswordHash = "123123313", Active = true
                    }
                }
            }), 
            Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
        );
        
        userServiceClient.Setup(x => x.FindAllAsync(It.IsAny<FindAllUserRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
            .Returns(fakeCall);

        roleService.Setup(x => x.GetRoles())
            .Returns(new List<Role>(){
                new (RoleConstant.Admin, UpperRoleCode: null),
                new (RoleConstant.User, UpperRoleCode: RoleConstant.Admin)
            });

        daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() {
                    { ConfigConstant.PasswordSalt, "" }
                }
            ));

        passwordHasher.Setup(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("123123313");

        //jwtToken
        configurationSection.Setup(c => c.Value).Returns("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e");

        configuration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);

        AuthenticationService authenticationService = new(logger.Object, daprClient.Object, passwordHasher.Object, userService.Object,
            configuration.Object, roleService.Object, _stringLocalizer, tenantCacheService.Object, _mapper);

        authenticationService.UserServiceClient = userServiceClient.Object;

        var auth = await authenticationService.AuthenticateAsync(userLogin);

        tenantCacheService.Verify(x => x.GetTenantAsync(It.IsAny<string>()), Times.Once);
        userService.Verify(x => x.FindAllAsync(It.IsAny<UserCriteria>()), Times.Once);
        roleService.Verify(x => x.GetRoles(), Times.Once);
        daprClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
        passwordHasher.Verify(x => x.GenerateHash(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.NotNull(auth);
        Assert.Equal(userId, auth.Id);
        Assert.NotNull(auth.Token);
    }
}